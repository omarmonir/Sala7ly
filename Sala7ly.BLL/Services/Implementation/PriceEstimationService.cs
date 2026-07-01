using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Sala7ly.BLL.DTOs.AiDTOs;
using Sala7ly.BLL.Services.Abstraction;
using Sala7ly.DAL.Enums;
using Sala7ly.DAL.Repositories.Abstraction;

namespace Sala7ly.BLL.Services.Implementation
{
    public class PriceEstimationService : BaseAiService, IPriceEstimationService
    {
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
                ?? throw new KeyNotFoundException("الطلب غير موجود."); // was a plain Exception — controller's 404 branch never fired

            // ── 1. Cache check ────────────────────────────────────────────────
            var cacheKey = $"price_estimate_{request.CategoryId}" +
                           $"_{request.Address?.District}" +
                           $"_{request.Urgency}";

            var cached = await _cache.GetStringAsync(cacheKey);
            if (cached != null)
                return JsonSerializer.Deserialize<PriceEstimateDto>(cached)!;

            // ── 2. Historical prices ──────────────────────────────────────────
            var historicalPrices = await _bidRepo
                .GetAcceptedPricesByCategoryAsync(request.CategoryId, limit: 50);

            var avgPrice = historicalPrices.Any() ? historicalPrices.Average() : 300m;
            var minPrice = historicalPrices.Any() ? historicalPrices.Min() : 150m;
            var maxPrice = historicalPrices.Any() ? historicalPrices.Max() : 600m;

            // ── 3. Build prompt ───────────────────────────────────────────────
            var userPrompt = PromptBuilder.PriceEstimationUser(
                categoryAr: request.Category.NameAr,
                description: request.AiSummary ?? request.Description,
                urgency: request.Urgency.ToString(),
                district: request.Address?.District ?? "غير محدد",
                avgPrice: avgPrice,
                minPrice: minPrice,
                maxPrice: maxPrice
            );

            // ── 4. Call the model (Haiku = cheapest / fastest) ────────────────
            var sw = Stopwatch.StartNew();
            var json = await CallAsync(HaikuModel, userPrompt, maxTokens: 300);
            sw.Stop();

            var result = ParseJson<PriceEstimateDto>(json);

            // ── 5. Persist AI price range on the request ──────────────────────
            request.SetAiData(
                request.AiSummary ?? request.Description,
                result.MinPrice,
                result.MaxPrice);
            await _requestRepo.SaveChangesAsync();

            // ── 6. Cache result ───────────────────────────────────────────────
            var ttlMinutes = int.Parse(_config["AI:Cache:PriceEstimateTtlMinutes"] ?? "60");
            await _cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(result),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(ttlMinutes)
                });

            // ── 7. Log AI interaction (now shared via BaseAiService) ───────────
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
    }
}