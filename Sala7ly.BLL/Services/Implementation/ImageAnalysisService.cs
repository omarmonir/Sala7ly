using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Sala7ly.BLL.DTOs.AiDTOs;
using Sala7ly.BLL.Services;
using Sala7ly.BLL.Services.Abstraction;
using Sala7ly.DAL.Entities;
using Sala7ly.DAL.Enums;
using Sala7ly.DAL.Repositories.Abstraction;

namespace Sala7ly.BLL.Services.Implementation
{
    public class ImageAnalysisService : BaseAiService, IImageAnalysisService
    {
        private readonly IServiceRequestRepository _requestRepo;
        private readonly IAiInteractionRepository _aiRepo;

        public ImageAnalysisService(
            IGitHubAiClient ai,
            IConfiguration config,
            IServiceRequestRepository requestRepo,
            IAiInteractionRepository aiRepo)
            : base(ai, config)
        {
            _requestRepo = requestRepo;
            _aiRepo = aiRepo;
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Analyse a single Base-64 encoded image.
        /// Does NOT require an existing service request — used during the
        /// "create request" flow before the request is saved.
        /// </summary>
        public async Task<ImageAnalysisDto> AnalyzeImageAsync(
            string base64Image,
            string mediaType = "image/jpeg")
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var prompt = PromptBuilder.ImageAnalysisUser();

            var raw = await CallWithImageAsync(
                model: SonnetModel,
                userPrompt: prompt,
                base64Image: base64Image,
                mediaType: mediaType,
                maxTokens: 500);

            sw.Stop();

            var result = ParseJson<ImageAnalysisDto>(raw);
            result.LatencyMs = (int)sw.ElapsedMilliseconds;
            return result;
        }

        public async Task<ImageAnalysisDto> AnalyzeImageFromFormFileAsync(IFormFile file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            await using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var bytes = ms.ToArray();
            var base64 = Convert.ToBase64String(bytes);
            var mediaType = file.ContentType ?? "image/jpeg";

            return await AnalyzeImageAsync(base64, mediaType);
        }

        /// <summary>
        /// Analyse an image that is already stored as a URL on the service
        /// request (post-upload flow).  Downloads, encodes, then calls the
        /// vision model, and persists both the DTO and an AI-interaction log.
        /// </summary>
        public async Task<ImageAnalysisDto> AnalyzeRequestImageAsync(
            int requestId,
            string imageUrl,
            string? userId = null)
        {
            var request = await _requestRepo.GetByIdAsync(requestId)
                ?? throw new Exception("الطلب غير موجود.");

            using var http = new HttpClient();
            var imageBytes = await http.GetByteArrayAsync(imageUrl);
            var base64 = Convert.ToBase64String(imageBytes);

            var mediaType = imageUrl.ToLower() switch
            {
                var u when u.EndsWith(".png") => "image/png",
                var u when u.EndsWith(".gif") => "image/gif",
                var u when u.EndsWith(".webp") => "image/webp",
                _ => "image/jpeg"
            };

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var prompt = PromptBuilder.ImageAnalysisUser();

            var raw = await CallWithImageAsync(
                model: SonnetModel,
                userPrompt: prompt,
                base64Image: base64,
                mediaType: mediaType,
                maxTokens: 500);

            sw.Stop();

            var result = ParseJson<ImageAnalysisDto>(raw);
            result.LatencyMs = (int)sw.ElapsedMilliseconds;

            var analysisJson = JsonSerializer.Serialize(result);
            request.SetAiRefinement(
                summary: result.SuggestedDescription,
                suggestedCategoryId: null,
                refinementJson: analysisJson);

            await _requestRepo.SaveChangesAsync();

            await _aiRepo.AddAsync(new Ai_Interaction
            {
                RequestId = requestId,
                UserId = userId ?? request.Profile?.UserId ?? string.Empty,
                InteractionType = AiInteractionType.image_analysis,
                ModelUsed = SonnetModel,
                PromptSnapshot = prompt,
                ResponseSnapshot = raw,
                ConfidenceScore = result.Confidence,
                LatencyMs = (int)sw.ElapsedMilliseconds,
                CreatedOn = DateTime.UtcNow
            });
            await _aiRepo.SaveChangesAsync();

            return result;
        }

        /// <summary>
        /// Analyse multiple images for one request and return an aggregated
        /// result (uses the first high-confidence analysis, or the last one).
        /// </summary>
        public async Task<ImageAnalysisDto> AnalyzeMultipleImagesAsync(
            int requestId,
            List<string> imageUrls,
            string? userId = null)
        {
            if (!imageUrls.Any())
                throw new ArgumentException("يجب تقديم صورة واحدة على الأقل.");

            var results = new List<ImageAnalysisDto>();

            foreach (var url in imageUrls.Take(3))
            {
                try
                {
                    var r = await AnalyzeRequestImageAsync(requestId, url, userId);
                    results.Add(r);
                }
                catch
                {
                    // Skip unreadable images rather than failing the whole call
                }
            }

            if (!results.Any())
                throw new Exception("تعذّر تحليل أي من الصور المُرفقة.");

            return results.MaxBy(r => r.Confidence)!;
        }
    }
}
