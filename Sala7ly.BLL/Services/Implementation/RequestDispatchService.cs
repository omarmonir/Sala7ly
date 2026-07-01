using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sala7ly.API.Hubs;
using Sala7ly.BLL.DTOs.AiDTOs;
using Sala7ly.BLL.Services.Abstraction;
using Sala7ly.DAL.Entities;
using Sala7ly.DAL.Enums;
using Sala7ly.DAL.Repositories.Abstraction;

namespace Sala7ly.BLL.Services.Implementation
{
    /// <summary>
    /// Resolves the best-fit category for a newly created ServiceRequest via
    /// an LLM sanity check, persists any override, and dispatches
    /// notifications (DB + SignalR) to every matching technician.
    /// </summary>
    public class RequestDispatchService : BaseAiService, IRequestDispatchService
    {
        private readonly IServiceCategoryRepository _categoryRepo;
        private readonly ITechnicianProfileRepository _technicianRepo;
        private readonly IServiceRequestRepository _requestRepo;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<BiddingHub> _biddingHub;
        private readonly ILogger<RequestDispatchService> _logger;

        public RequestDispatchService(
            IGitHubAiClient ai,
            IConfiguration config,
            IAiInteractionRepository aiInteractionRepo,
            IServiceCategoryRepository categoryRepo,
            ITechnicianProfileRepository technicianRepo,
            IServiceRequestRepository requestRepo,
            INotificationService notificationService,
            IHubContext<BiddingHub> biddingHub,
            ILogger<RequestDispatchService> logger)
            : base(ai, config, aiInteractionRepo)
        {
            _categoryRepo = categoryRepo;
            _technicianRepo = technicianRepo;
            _requestRepo = requestRepo;
            _notificationService = notificationService;
            _biddingHub = biddingHub;
            _logger = logger;
        }

        public async Task<SmartMatchingResultDto> MatchAndNotifyAsync(
            ServiceRequest request,
            string customerUserId)
        {
            var result = new SmartMatchingResultDto
            {
                OriginalCategoryId = request.CategoryId,
                ResolvedCategoryId = request.CategoryId,
                CategoryWasOverridden = false
            };

            try
            {
                var allCategories = (await _categoryRepo.GetAllAsync()).ToList();
                if (!allCategories.Any())
                {
                    _logger.LogWarning("RequestDispatch: no active categories found — skipping AI verification.");
                    await DispatchNotificationsAsync(request, result, customerUserId);
                    return result;
                }

                var categoryList = allCategories
                    .Select(c => $"{c.Id}:{c.NameAr}")
                    .ToList();

                var aiSuggestedCategoryId = await AskAiForBestCategoryAsync(
                    request, customerUserId, request.CategoryId, categoryList);

                var originalCategoryId = request.CategoryId;

                if (aiSuggestedCategoryId.HasValue
                    && aiSuggestedCategoryId.Value != request.CategoryId
                    && allCategories.Any(c => c.Id == aiSuggestedCategoryId.Value))
                {
                    result.ResolvedCategoryId = aiSuggestedCategoryId.Value;
                    result.CategoryWasOverridden = true;

                    var originalName = allCategories
                        .FirstOrDefault(c => c.Id == originalCategoryId)?.NameAr ?? originalCategoryId.ToString();
                    var suggestedName = allCategories
                        .First(c => c.Id == aiSuggestedCategoryId.Value).NameAr;

                    result.AiMatchReason = $"AI اقترح الفئة «{suggestedName}» بدلاً من «{originalName}».";

                    request.UpdateCategory(aiSuggestedCategoryId.Value);
                    request.SetAiRefinement(
                        summary: $"AI matched request to category: {suggestedName}",
                        suggestedCategoryId: aiSuggestedCategoryId.Value,
                        refinementJson: BuildRefinementJson(
                            request, originalCategoryId, aiSuggestedCategoryId.Value, suggestedName));

                    _requestRepo.Update(request);
                    await _requestRepo.SaveChangesAsync();

                    _logger.LogInformation(
                        "RequestDispatch: request {RequestId} category overridden " +
                        "{OriginalId} ({OriginalName}) -> {SuggestedId} ({SuggestedName}).",
                        request.Id, originalCategoryId, originalName,
                        aiSuggestedCategoryId.Value, suggestedName);
                }
                else
                {
                    result.AiMatchReason = "الفئة المختارة من قِبل العميل مناسبة.";
                }

                await DispatchNotificationsAsync(request, result, customerUserId);
            }
            catch (Exception ex)
            {
                // Non-critical: log and return what we have so request creation
                // is never blocked by an AI or matching failure.
                _logger.LogError(ex, "RequestDispatch: unexpected error for request {Id}.", request.Id);

                try
                {
                    await DispatchNotificationsAsync(request, result, customerUserId);
                }
                catch (Exception innerEx)
                {
                    _logger.LogError(innerEx,
                        "RequestDispatch: notification dispatch also failed for request {Id}.", request.Id);
                }
            }

            return result;
        }

        /// <summary>
        /// Asks the LLM to pick the best-matching category from the supplied
        /// list. Returns null when the AI cannot decide or the call fails —
        /// routed through IGitHubAiClient/BaseAiService instead of a raw,
        /// hand-rolled HttpClient call to the model endpoint.
        /// </summary>
        private async Task<int?> AskAiForBestCategoryAsync(
            ServiceRequest request,
            string customerUserId,
            int customerChosenCategoryId,
            List<string> categoryList)
        {
            var prompt = PromptBuilder.CategoryVerificationUser(
                request.Title, request.Description, customerChosenCategoryId, categoryList);

            var sw = Stopwatch.StartNew();
            try
            {
                var raw = await CallAsync(HaikuModel, prompt, maxTokens: 60);
                sw.Stop();

                var parsed = ParseJson<CategoryVerificationResponse>(raw);

                await LogInteractionAsync(
                    request.Id, customerUserId, AiInteractionType.categorization,
                    HaikuModel, prompt, raw, confidence: null, (int)sw.ElapsedMilliseconds);

                return parsed.SuggestedCategoryId > 0 ? parsed.SuggestedCategoryId : null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "RequestDispatch: AI category verification failed; " +
                    "falling back to customer choice {Id}.", customerChosenCategoryId);
                return null;
            }
        }

        private async Task DispatchNotificationsAsync(
            ServiceRequest request,
            SmartMatchingResultDto result,
            string customerUserId)
        {
            var technicianUserIds = await _technicianRepo
                .GetUserIdsByAnyCategoryAsync(new[] { result.ResolvedCategoryId });

            result.NotifiedTechnicianCount = technicianUserIds.Count;

            if (technicianUserIds.Count == 0)
            {
                _logger.LogInformation(
                    "RequestDispatch: no technicians found for category {CatId} (request {ReqId}).",
                    result.ResolvedCategoryId, request.Id);
                return;
            }

            await _notificationService.NotifyMultipleUsersAsync(
                userIds: technicianUserIds,
                type: NotificationType.system,
                title: "طلب خدمة جديد يناسب تخصصك 🔔",
                body: $"طلب جديد: {request.Title}",
                actorId: customerUserId,
                metadata: BuildMetadata(request, result));

            var broadcastPayload = new
            {
                requestId = request.Id,
                categoryId = result.ResolvedCategoryId,
                title = request.Title,
                urgency = request.Urgency.ToString(),
                isEmergency = request.IsEmergency,
                scheduledAt = request.ScheduledAt,
                aiMatchReason = result.AiMatchReason,
                categoryOverridden = result.CategoryWasOverridden
            };

            var hubTasks = technicianUserIds
                .Select(uid => _biddingHub.Clients.User(uid).SendAsync("NewRequestAvailable", broadcastPayload));

            await Task.WhenAll(hubTasks);

            _logger.LogInformation(
                "RequestDispatch: request {Id} dispatched to {Count} technician(s) in category {CatId}.",
                request.Id, technicianUserIds.Count, result.ResolvedCategoryId);
        }

        private static string BuildMetadata(ServiceRequest request, SmartMatchingResultDto result)
            => JsonSerializer.Serialize(new
            {
                requestId = request.Id,
                categoryId = result.ResolvedCategoryId,
                originalCategoryId = result.OriginalCategoryId,
                categoryOverridden = result.CategoryWasOverridden,
                aiMatchReason = result.AiMatchReason
            }, new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });

        private static string BuildRefinementJson(
            ServiceRequest request,
            int originalCategoryId,
            int suggestedCategoryId,
            string suggestedCategoryName)
            => JsonSerializer.Serialize(new
            {
                source = "RequestDispatch",
                originalCategoryId,
                finalCategoryId = suggestedCategoryId,
                suggestedCategoryId,
                suggestedCategoryName,
                categoryOverridden = originalCategoryId != suggestedCategoryId,
                requestTitle = request.Title,
                generatedAt = DateTime.UtcNow
            }, new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });

        private sealed class CategoryVerificationResponse
        {
            public int SuggestedCategoryId { get; set; }
        }
    }
}