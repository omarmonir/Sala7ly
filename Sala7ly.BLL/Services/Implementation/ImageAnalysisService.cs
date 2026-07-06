using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Sala7ly.BLL.DTOs.AiDTOs;
using Sala7ly.BLL.Services;
using Sala7ly.BLL.Services.Abstraction;
using Sala7ly.DAL.Enums;
using Sala7ly.DAL.Repositories.Abstraction;

namespace Sala7ly.BLL.Services.Implementation
{
    public class ImageAnalysisService : BaseAiService, IImageAnalysisService
    {
        private const string ImageDownloadClientName = "ImageDownloader";
        private const int MaxImagesPerRequest = 3;

        private readonly IServiceRequestRepository _requestRepo;
        private readonly IHttpClientFactory _httpClientFactory;

        public ImageAnalysisService(
            IGitHubAiClient ai,
            IConfiguration config,
            IAiInteractionRepository aiInteractionRepo,
            IServiceRequestRepository requestRepo,
            IHttpClientFactory httpClientFactory)
            : base(ai, config, aiInteractionRepo)
        {
            _requestRepo = requestRepo;
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Analyse a single Base-64 encoded image. Does NOT require an
        /// existing service request — used during the "create request" flow
        /// before the request is saved.
        /// </summary>
        public async Task<ImageAnalysisDto> AnalyzeImageAsync(
            string base64Image,
            string mediaType = "image/jpeg")
        {
            var sw = Stopwatch.StartNew();
            var prompt = PromptBuilder.ImageAnalysisUser();

            var raw = await CallWithImageAsync(SonnetModel, prompt, base64Image, mediaType, maxTokens: 500);
            sw.Stop();

            var result = ParseJson<ImageAnalysisDto>(raw);
            result.LatencyMs = (int)sw.ElapsedMilliseconds;
            return result;
        }

        /// <summary>
        /// Analyse an image uploaded as IFormFile. Converts the file to
        /// base64 and delegates to AnalyzeImageAsync.
        /// </summary>
        public async Task<ImageAnalysisDto> AnalyzeImageFromFormFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("يجب تقديم صورة صالحة.");

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var base64 = Convert.ToBase64String(ms.ToArray());
            var mediaType = file.ContentType ?? "image/jpeg";

            return await AnalyzeImageAsync(base64, mediaType);
        }

        /// <summary>
        /// Analyse an image already stored as a URL on the service request
        /// (post-upload flow). Downloads, encodes, calls the vision model,
        /// and persists both the DTO and an AI-interaction audit log.
        /// </summary>
        public async Task<ImageAnalysisDto> AnalyzeRequestImageAsync(
            int requestId,
            string imageUrl,
            string? userId = null)
        {
            var request = await _requestRepo.GetByIdAsync(requestId)
                ?? throw new KeyNotFoundException("الطلب غير موجود.");

            // Use a managed, named HttpClient instead of `new HttpClient()`
            // (previous code created one per call — socket-exhaustion risk).
            var http = _httpClientFactory.CreateClient(ImageDownloadClientName);
            var imageBytes = await http.GetByteArrayAsync(imageUrl);
            var base64 = Convert.ToBase64String(imageBytes);
            var mediaType = ResolveMediaType(imageUrl);

            var sw = Stopwatch.StartNew();
            var prompt = PromptBuilder.ImageAnalysisUser();

            var raw = await CallWithImageAsync(SonnetModel, prompt, base64, mediaType, maxTokens: 500);
            sw.Stop();

            var result = ParseJson<ImageAnalysisDto>(raw);
            result.LatencyMs = (int)sw.ElapsedMilliseconds;

            request.SetAiRefinement(
                summary: result.SuggestedDescription,
                suggestedCategoryId: null,   // image analysis doesn't pick a category
                refinementJson: JsonSerializer.Serialize(result));

            await _requestRepo.SaveChangesAsync();

            await LogInteractionAsync(
                requestId,
                userId ?? request.Profile?.UserId,
                AiInteractionType.image_analysis,
                SonnetModel,
                prompt,
                raw,
                result.Confidence,
                (int)sw.ElapsedMilliseconds);

            return result;
        }

        /// <summary>
        /// Analyse multiple images for one request; returns the
        /// highest-confidence result. Unreadable/unanalyzable images are
        /// skipped, but a missing request now correctly propagates as a
        /// 404 instead of being swallowed as "no images could be analyzed".
        /// </summary>
        public async Task<ImageAnalysisDto> AnalyzeMultipleImagesAsync(
            int requestId,
            List<string> imageUrls,
            string? userId = null)
        {
            if (imageUrls == null || !imageUrls.Any())
                throw new ArgumentException("يجب تقديم صورة واحدة على الأقل.");

            var results = new List<ImageAnalysisDto>();

            foreach (var url in imageUrls.Take(MaxImagesPerRequest))
            {
                try
                {
                    results.Add(await AnalyzeRequestImageAsync(requestId, url, userId));
                }
                catch (KeyNotFoundException)
                {
                    throw; // the request itself is missing — no point trying other URLs
                }
                catch
                {
                    // Skip unreadable/unanalyzable images rather than failing the whole call
                }
            }

            if (!results.Any())
                throw new InvalidOperationException("تعذّر تحليل أي من الصور المُرفقة.");

            return results.MaxBy(r => r.Confidence)!;
        }

        private static string ResolveMediaType(string imageUrl) => imageUrl.ToLowerInvariant() switch
        {
            var u when u.EndsWith(".png") => "image/png",
            var u when u.EndsWith(".gif") => "image/gif",
            var u when u.EndsWith(".webp") => "image/webp",
            _ => "image/jpeg"
        };
    }
}