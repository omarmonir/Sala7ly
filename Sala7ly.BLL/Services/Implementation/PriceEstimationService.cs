using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Sala7ly.BLL.DTOs.AiDTOs;
using Sala7ly.BLL.Services;
using Sala7ly.BLL.Services.Abstraction;
using Sala7ly.DAL.Enums;
using Sala7ly.DAL.Repositories.Abstraction;

namespace Sala7ly.BLL.Services.Implementation
{
    public class PriceEstimationService : BaseAiService, IPriceEstimationService
    {
        private const int MaxSimilarJobs = 15;

        private readonly IServiceRequestRepository _requestRepo;
        private readonly IBidRepository _bidRepo;
        private readonly IDistributedCache _cache;

        public PriceEstimationService(
            IGitHubAiClient ai,
            IConfiguration config,
            IAiInteractionRepository aiInteractionRepo,
            IServiceRequestRepository requestRepo,
            IBidRepository bidRepo,
            IDistributedCache cache)
            : base(ai, config, aiInteractionRepo)
        {
            _requestRepo = requestRepo;
            _bidRepo = bidRepo;
            _cache = cache;
        }

        public async Task<PriceEstimateDto> EstimateAsync(int requestId)
        {
            var request = await _requestRepo.GetByIdWithDetailsAsync(requestId)
                ?? throw new KeyNotFoundException("الطلب غير موجود.");

            // ── 1. Cache check ────────────────────────────────────────────
            var descriptionHash = Convert.ToHexString(
                System.Security.Cryptography.SHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes(
                        request.Description ?? string.Empty)))
                .Substring(0, 16);

            var cacheKey = $"price_estimate_{request.CategoryId}" +
                           $"_{request.Address?.District}" +
                           $"_{request.Urgency}" +
                           $"_{descriptionHash}";

            var cached = await _cache.GetStringAsync(cacheKey);
            if (cached != null)
                return JsonSerializer.Deserialize<PriceEstimateDto>(cached)!;

            // ── 2. Historical prices (aggregate) ─────────────────────────
            var historicalPrices = await _bidRepo
                .GetAcceptedPricesByCategoryAsync(request.CategoryId, limit: 50);

            var avgPrice = historicalPrices.Any() ? historicalPrices.Average() : 300m;
            var minPrice = historicalPrices.Any() ? historicalPrices.Min() : 150m;
            var maxPrice = historicalPrices.Any() ? historicalPrices.Max() : 600m;

            // ── 3. RAG: fetch similar completed jobs ──────────────────────
            var similarRequests = await _requestRepo
                .GetCompletedByCategoryAsync(request.CategoryId, MaxSimilarJobs);

            var similarJobs = similarRequests
                .Select(r => BuildJobSummary(r))
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Take(8)
                .ToList();

            // ── 4. Build enriched prompt ──────────────────────────────────
            var userPrompt = similarJobs.Count > 0
                ? PromptBuilder.PriceEstimationWithContextUser(
                    categoryAr: request.Category.NameAr,
                    description: request.AiSummary ?? request.Description,
                    urgency: request.Urgency.ToString(),
                    district: request.Address?.District ?? "غير محدد",
                    avgPrice: avgPrice,
                    minPrice: minPrice,
                    maxPrice: maxPrice,
                    similarJobs: similarJobs)
                : PromptBuilder.PriceEstimationUser(
                    categoryAr: request.Category.NameAr,
                    description: request.AiSummary ?? request.Description,
                    urgency: request.Urgency.ToString(),
                    district: request.Address?.District ?? "غير محدد",
                    avgPrice: avgPrice,
                    minPrice: minPrice,
                    maxPrice: maxPrice);  // fallback: original prompt

            // ── 5. Call the model ─────────────────────────────────────────
            var sw = Stopwatch.StartNew();
            var json = await CallAsync(HaikuModel, userPrompt, maxTokens: 300);
            sw.Stop();

            var result = ParseJson<PriceEstimateDto>(json);

            // ── 6. Persist AI price range on the request ──────────────────
            request.SetAiData(
                request.AiSummary ?? request.Description,
                result.MinPrice,
                result.MaxPrice);
            await _requestRepo.SaveChangesAsync();

            // ── 7. Cache result ───────────────────────────────────────────
            var ttlMinutes = int.Parse(
                _config["AI:Cache:PriceEstimateTtlMinutes"] ?? "60");
            await _cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(result),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(ttlMinutes)
                });

            // ── 8. Log AI interaction ─────────────────────────────────────
            await LogInteractionAsync(
                requestId,
                request.Profile?.UserId,
                AiInteractionType.price_estimation,
                HaikuModel,
                userPrompt,
                json,
                (float)result.Confidence,
                (int)sw.ElapsedMilliseconds);

            return result;
        }

        /// <summary>
        /// Converts a completed ServiceRequest into a one-line summary for the
        /// price estimation prompt. Uses accepted bid price when available.
        /// Example output:
        ///   "وصف: استبدال ضاغط تكييف | السعر المقبول: 750 ج | مدة التنفيذ: 2 ساعات"
        /// </summary>
        private static string BuildJobSummary(DAL.Entities.ServiceRequest r)
        {
            var parts = new List<string>();

            var desc = r.AiSummary ?? r.Description;
            if (!string.IsNullOrWhiteSpace(desc))
                parts.Add($"وصف: {desc[..Math.Min(80, desc.Length)]}");

            if (r.SelectedBid != null && r.SelectedBid.Price > 0)
                parts.Add($"السعر المقبول: {r.SelectedBid.Price} ج");
            else if (r.AiPriceMin > 0)
                parts.Add($"السعر المقدّر: {r.AiPriceMin}-{r.AiPriceMax} ج");

            if (r.StartedAt.HasValue && r.CompletedAt.HasValue)
            {
                var hours = (r.CompletedAt.Value - r.StartedAt.Value).TotalHours;
                if (hours > 0 && hours < 24)
                    parts.Add($"مدة التنفيذ: {hours:F1} ساعة");
            }

            return string.Join(" | ", parts);
        }
    }
}