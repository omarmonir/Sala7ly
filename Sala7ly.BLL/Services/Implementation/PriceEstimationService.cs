using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Sala7ly.BLL.DTOs.AiDTOs;
using Sala7ly.BLL.Services;
using Sala7ly.BLL.Services.Abstraction;
using Sala7ly.DAL.Entities;
using Sala7ly.DAL.Enums;
using Sala7ly.DAL.Repositories.Abstraction;

namespace Sala7ly.BLL.Services.Implementation
{
    public class PriceEstimationService : BaseAiService, IPriceEstimationService
    {
        private readonly IServiceRequestRepository _requestRepo;
        private readonly IBidRepository _bidRepo;
        private readonly IDistributedCache _cache;
        private readonly IAiInteractionRepository _aiRepo;

        public PriceEstimationService(
            IGitHubAiClient ai,           // ← IGitHubAiClient, not AnthropicClient
            IConfiguration config,
            IServiceRequestRepository requestRepo,
            IBidRepository bidRepo,
            IDistributedCache cache,
            IAiInteractionRepository aiRepo)
            : base(ai, config)
        {
            _requestRepo = requestRepo;
            _bidRepo = bidRepo;
            _cache = cache;
            _aiRepo = aiRepo;
        }

        public async Task<PriceEstimateDto> EstimateAsync(int requestId)
        {
            var request = await _requestRepo.GetByIdWithDetailsAsync(requestId)
                ?? throw new Exception("الطلب غير موجود.");

            // ── 1. Cache check ────────────────────────────────────────────────
            var descriptionHash = Convert.ToHexString(
                System.Security.Cryptography.SHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes(request.Description ?? string.Empty)))
                .Substring(0, 16);

            var cacheKey = $"price_estimate_{request.CategoryId}" +
                           $"_{request.Address?.District}" +
                           $"_{request.Urgency}" +
                           $"_{descriptionHash}";

            var cached = await _cache.GetStringAsync(cacheKey);
            if (cached != null)
                return JsonSerializer.Deserialize<PriceEstimateDto>(cached)!;

            // ── 2. Historical prices ──────────────────────────────────────────
            var historicalPrices = await _bidRepo
                .GetAcceptedPricesByCategoryAsync(request.CategoryId, limit: 50);

            var avgPrice = historicalPrices.Any() ? (decimal)historicalPrices.Average() : 300m;
            var minPrice = historicalPrices.Any() ? (decimal)historicalPrices.Min() : 150m;
            var maxPrice = historicalPrices.Any() ? (decimal)historicalPrices.Max() : 600m;

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

            // ── 4. Call GitHub Models (Haiku = cheapest / fastest) ───────────
            var sw = System.Diagnostics.Stopwatch.StartNew();
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

            // ── 7. Log AI interaction ─────────────────────────────────────────
            await _aiRepo.AddAsync(new Ai_Interaction
            {
                RequestId = requestId,
                InteractionType = AiInteractionType.price_estimation,
                UserId = request.Profile?.UserId ?? "",
                ModelUsed = HaikuModel,
                PromptSnapshot = userPrompt,
                ResponseSnapshot = json,
                ConfidenceScore = (float)result.Confidence,
                LatencyMs = (int)sw.ElapsedMilliseconds,
                CreatedOn = DateTime.UtcNow
            });
            await _aiRepo.SaveChangesAsync();

            return result;
        }
    }
}
