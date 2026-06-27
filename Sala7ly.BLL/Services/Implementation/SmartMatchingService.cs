using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Sala7ly.API.Hubs;
using Sala7ly.BLL.DTOs.AiDTOs;
using Sala7ly.BLL.Services.Abstraction;
using Sala7ly.DAL.Entities;
using Sala7ly.DAL.Enums;
using Sala7ly.DAL.Repositories.Abstraction;
using System.Net.Http.Json;
using System.Text.Json;

namespace Sala7ly.BLL.Services.Implementation
{
    /// <summary>
    /// AI-powered smart technician matching service.
    ///
    /// When a customer creates a ServiceRequest the service:
    ///   1. Sends the request title + description to GitHub Models (gpt-4o-mini)
    ///      to verify that the customer-selected category is the best fit.
    ///   2. If the AI suggests a better category it persists the override on the
    ///      ServiceRequest via SetAiRefinement() so the data is never lost.
    ///   3. Queries the TechnicianProfile repository for every approved technician
    ///      whose TechnicianCategory rows include the resolved category.
    ///   4. Saves a Notification row for each matching technician and pushes a
    ///      real-time event to any connected clients via NotificationHub.
    ///   5. Also broadcasts a SignalR "NewRequestAvailable" event on BiddingHub
    ///      so technicians who are already watching the bid board see it instantly.
    /// </summary>
    public class SmartMatchingService : ISmartMatchingService
    {
        // ── GitHub Models settings (same as RequestRefinerService) ────────────
        private const string Model = "gpt-4o-mini";
        private const string Endpoint = "https://models.inference.ai.azure.com/chat/completions";

        private readonly HttpClient _http;
        private readonly IServiceCategoryRepository _categoryRepo;
        private readonly ITechnicianProfileRepository _technicianRepo;
        private readonly IServiceRequestRepository _requestRepo;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<BiddingHub> _biddingHub;
        private readonly ILogger<SmartMatchingService> _logger;

        public SmartMatchingService(
            IHttpClientFactory httpFactory,
            IServiceCategoryRepository categoryRepo,
            ITechnicianProfileRepository technicianRepo,
            IServiceRequestRepository requestRepo,
            INotificationService notificationService,
            IHubContext<BiddingHub> biddingHub,
            ILogger<SmartMatchingService> logger)
        {
            _http = httpFactory.CreateClient("GitHubModels");
            _categoryRepo = categoryRepo;
            _technicianRepo = technicianRepo;
            _requestRepo = requestRepo;
            _notificationService = notificationService;
            _biddingHub = biddingHub;
            _logger = logger;
        }

        // ── Public entry point ────────────────────────────────────────────────

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
                // ── Step 1: load all active categories for the AI prompt ───────
                var allCategories = (await _categoryRepo.GetAllAsync()).ToList();
                if (!allCategories.Any())
                {
                    _logger.LogWarning(
                        "SmartMatching: no active categories found — skipping AI verification.");
                    await DispatchNotificationsAsync(request, result, customerUserId);
                    return result;
                }

                // Build the "id:name" list the AI understands
                // (same format as AiController.GetCategoryListAsync)
                var categoryList = allCategories
                    .Select(c => $"{c.Id}:{c.NameAr}")
                    .ToList();

                // ── Step 2: ask AI to verify / suggest the best category ───────
                int? aiSuggestedCategoryId = await AskAiForBestCategoryAsync(
                    request.Title,
                    request.Description,
                    request.CategoryId,
                    categoryList);

                // ── Step 3: apply override when AI differs from customer ────────
                int originalCategoryId = request.CategoryId;   // snapshot before any change

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

                    result.AiMatchReason =
                        $"AI اقترح الفئة «{suggestedName}» بدلاً من «{originalName}».";

                    // ── THE FIX: update the actual CategoryId on the entity ────
                    // Without this, technician feeds (which filter by CategoryId)
                    // would never show this request to the correct technicians.
                    request.UpdateCategory(aiSuggestedCategoryId.Value);

                    // Store the AI metadata fields
                    request.SetAiRefinement(
                        summary: $"AI matched request to category: {suggestedName}",
                        suggestedCategoryId: aiSuggestedCategoryId.Value,
                        refinementJson: BuildRefinementJson(
                                                request,
                                                originalCategoryId,
                                                aiSuggestedCategoryId.Value,
                                                suggestedName));

                    _requestRepo.Update(request);
                    await _requestRepo.SaveChangesAsync();

                    // Detailed log: original → AI suggestion → final saved value
                    _logger.LogInformation(
                        "SmartMatching: request {RequestId} | " +
                        "OriginalCategory: {OriginalId} ({OriginalName}) | " +
                        "AiSuggested: {SuggestedId} ({SuggestedName}) | " +
                        "FinalSavedCategory: {FinalId} ({FinalName}) | " +
                        "CategoryOverridden: true",
                        request.Id,
                        originalCategoryId, originalName,
                        aiSuggestedCategoryId.Value, suggestedName,
                        request.CategoryId, suggestedName);
                }
                else
                {
                    var categoryName = allCategories
                        .FirstOrDefault(c => c.Id == request.CategoryId)?.NameAr
                        ?? request.CategoryId.ToString();

                    result.AiMatchReason = "الفئة المختارة من قِبل العميل مناسبة.";

                    _logger.LogInformation(
                        "SmartMatching: request {RequestId} | " +
                        "OriginalCategory: {OriginalId} ({OriginalName}) | " +
                        "AiSuggested: {SuggestedId} ({SuggestedName}) | " +
                        "FinalSavedCategory: {FinalId} ({FinalName}) | " +
                        "CategoryOverridden: false",
                        request.Id,
                        originalCategoryId, categoryName,
                        aiSuggestedCategoryId ?? originalCategoryId, categoryName,
                        request.CategoryId, categoryName);
                }

                // ── Step 4: notify matched technicians ────────────────────────
                await DispatchNotificationsAsync(request, result, customerUserId);
            }
            catch (Exception ex)
            {
                // Non-critical: log and return what we have so the create flow
                // is never blocked by a matching / AI failure.
                _logger.LogError(ex,
                    "SmartMatching: unexpected error for request {Id}.", request.Id);

                // Still try a best-effort notification with the original category
                try
                {
                    await DispatchNotificationsAsync(request, result, customerUserId);
                }
                catch (Exception innerEx)
                {
                    _logger.LogError(innerEx,
                        "SmartMatching: notification dispatch also failed for request {Id}.",
                        request.Id);
                }
            }

            return result;
        }

        // ── Private: AI call ──────────────────────────────────────────────────

        /// <summary>
        /// Sends the request title/description to GitHub Models and asks it to
        /// pick the best-matching category from the supplied list.
        /// Returns null when the AI cannot decide or the call fails.
        /// </summary>
        private async Task<int?> AskAiForBestCategoryAsync(
            string title,
            string description,
            int customerChosenCategoryId,
            List<string> categoryList)
        {
            var prompt = $$"""
                أنت مساعد متخصص في تصنيف طلبات الصيانة المنزلية في مصر.

                الطلب:
                العنوان: "{{title}}"
                الوصف: "{{description}}"

                الفئة التي اختارها العميل: {{customerChosenCategoryId}}

                الفئات المتاحة (id:الاسم):
                {{string.Join("، ", categoryList)}}

                المطلوب:
                - حدد الفئة الأنسب لهذا الطلب من القائمة أعلاه.
                - إذا كانت فئة العميل صحيحة، أعد نفس الرقم.
                - أعد رقم الـ id فقط بدون أي نص إضافي.

                رد بـ JSON فقط:
                {"suggestedCategoryId": <رقم صحيح>}
                """;

            try
            {
                var body = new
                {
                    model = Model,
                    messages = new[] { new { role = "user", content = prompt } },
                    temperature = 0.1,
                    max_tokens = 60
                };

                var response = await _http.PostAsJsonAsync(Endpoint, body);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                var content = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? "";

                // Strip markdown fences if the model adds them
                content = content
                    .Replace("```json", "")
                    .Replace("```", "")
                    .Trim();

                using var resultDoc = JsonDocument.Parse(content);
                var categoryId = resultDoc.RootElement
                    .GetProperty("suggestedCategoryId")
                    .GetInt32();

                return categoryId > 0 ? categoryId : (int?)null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "SmartMatching: AI category verification failed; " +
                    "falling back to customer choice {Id}.", customerChosenCategoryId);
                return null;   // graceful fallback
            }
        }

        // ── Private: notification dispatch ────────────────────────────────────

        /// <summary>
        /// Finds all approved technicians who cover the resolved category and
        /// sends each a persisted notification + SignalR real-time event.
        /// </summary>
        private async Task DispatchNotificationsAsync(
            ServiceRequest request,
            SmartMatchingResultDto result,
            string customerUserId)
        {
            // 1. Find matching technician User IDs
            var technicianUserIds = await _technicianRepo
                .GetUserIdsByAnyCategoryAsync(new[] { result.ResolvedCategoryId });

            result.NotifiedTechnicianCount = technicianUserIds.Count;

            if (technicianUserIds.Count == 0)
            {
                _logger.LogInformation(
                    "SmartMatching: no technicians found for category {CatId} " +
                    "(request {ReqId}).",
                    result.ResolvedCategoryId, request.Id);
                return;
            }

            // 2. Persisted + SignalR push notifications (fan-out)
            await _notificationService.NotifyMultipleUsersAsync(
                userIds: technicianUserIds,
                type: NotificationType.system,
                title: "طلب خدمة جديد يناسب تخصصك 🔔",
                body: $"طلب جديد: {request.Title}",
                actorId: customerUserId,
                metadata: BuildMetadata(request, result));

            _logger.LogInformation(
                "SmartMatching: request {RequestId} | " +
                "FinalCategory: {CategoryId} | " +
                "MatchedTechnicians: {Count} | " +
                "NotificationsDispatched: {Count}",
                request.Id,
                result.ResolvedCategoryId,
                technicianUserIds.Count,
                technicianUserIds.Count);

            // 3. Real-time BiddingHub broadcast so technicians on the bid board
            //    see the new card without refreshing.
            //    Each technician is targeted individually via their UserId group.
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
                .Select(uid => _biddingHub.Clients
                    .User(uid)
                    .SendAsync("NewRequestAvailable", broadcastPayload));

            await Task.WhenAll(hubTasks);

            _logger.LogInformation(
                "SmartMatching: request {Id} dispatched to {Count} technician(s) " +
                "in category {CatId}.",
                request.Id, technicianUserIds.Count, result.ResolvedCategoryId);
        }

        // ── Private: helpers ──────────────────────────────────────────────────

        private static string BuildMetadata(ServiceRequest request, SmartMatchingResultDto result)
            => JsonSerializer.Serialize(new
            {
                requestId = request.Id,
                categoryId = result.ResolvedCategoryId,
                originalCategoryId = result.OriginalCategoryId,
                categoryOverridden = result.CategoryWasOverridden,
                aiMatchReason = result.AiMatchReason
            }, new JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

        private static string BuildRefinementJson(
            ServiceRequest request,
            int originalCategoryId,
            int suggestedCategoryId,
            string suggestedCategoryName)
            => JsonSerializer.Serialize(new
            {
                source = "SmartMatching",
                originalCategoryId,                        // what customer chose
                finalCategoryId = suggestedCategoryId,  // what was actually saved
                suggestedCategoryId,
                suggestedCategoryName,
                categoryOverridden = originalCategoryId != suggestedCategoryId,
                requestTitle = request.Title,
                generatedAt = DateTime.UtcNow
            }, new JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
    }
}