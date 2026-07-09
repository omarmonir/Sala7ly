using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Sala7ly.BLL.DTOs.AiDTOs;
using Sala7ly.BLL.Services;
using Sala7ly.BLL.Services.Abstraction;
using Sala7ly.DAL.Entities;
using Sala7ly.DAL.Enums;
using Sala7ly.DAL.Repositories.Abstraction;

namespace Sala7ly.BLL.Services.Implementation
{
    public class RequestRefinerService : BaseAiService, IRequestRefinerService
    {
        private const int MaxFollowUpQuestions = 3;
        private const int MaxHistoricalRequests = 10;

        private readonly IServiceRequestRepository _requestRepo;

        public RequestRefinerService(
            IGitHubAiClient ai,
            IConfiguration config,
            IAiInteractionRepository aiInteractionRepo,
            IServiceRequestRepository requestRepo)
            : base(ai, config, aiInteractionRepo)
        {
            _requestRepo = requestRepo;
        }

        // ── Step 1: ask one follow-up question (RAG-enhanced) ────────────
        public async Task<FollowUpDto> AskFollowUpAsync(
            FollowUpRequestDto dto, string? userId = null)
        {
            if (dto.PreviousAnswers.Count >= MaxFollowUpQuestions)
                return new FollowUpDto { Question = "", IsComplete = true };

            // ── RAG: detect category from description and fetch history ───
            var historicalPatterns = await BuildFollowUpPatternsAsync(
                dto.RawDescription, dto.Categories);

            // ── Build enriched prompt ─────────────────────────────────────
            var prompt = historicalPatterns.Count > 0
                ? PromptBuilder.RequestFollowUpWithContextUser(
                    dto.RawDescription,
                    dto.Categories,
                    dto.PreviousAnswers,
                    historicalPatterns)
                : PromptBuilder.RequestFollowUpUser(
                    dto.RawDescription,
                    dto.Categories,
                    dto.PreviousAnswers);  // fallback: no history yet

            var sw = Stopwatch.StartNew();
            var raw = await CallAsync(HaikuModel, prompt, maxTokens: 200);
            sw.Stop();

            await LogInteractionAsync(
                requestId: null, userId,
                AiInteractionType.request_refine,
                HaikuModel, prompt, raw,
                confidence: null, (int)sw.ElapsedMilliseconds);

            try
            {
                return ParseJson<FollowUpDto>(raw);
            }
            catch
            {
                return new FollowUpDto { Question = "", IsComplete = true };
            }
        }

        // ── Step 2: refine description (RAG-enhanced) ────────────────────
        public async Task<RefineResultDto> RefineAsync(
            RefineRequestDto dto, List<string> allAnswers, string? userId = null)
        {
            // ── RAG: fetch similar completed request descriptions ─────────
            var exampleDescriptions = await BuildRefineExamplesAsync(dto.Categories);

            // ── Build enriched prompt ─────────────────────────────────────
            var prompt = exampleDescriptions.Count > 0
                ? PromptBuilder.RequestRefineWithContextUser(
                    dto.RawDescription,
                    dto.Categories,
                    allAnswers,
                    exampleDescriptions)
                : PromptBuilder.RequestRefineUser(
                    dto.RawDescription,
                    dto.Categories,
                    allAnswers);  // fallback: no history yet

            var sw = Stopwatch.StartNew();
            var raw = await CallAsync(HaikuModel, prompt, maxTokens: 300);
            sw.Stop();

            Console.WriteLine("========== AI RESPONSE ==========");
            Console.WriteLine(raw);
            Console.WriteLine("=================================");

            await LogInteractionAsync(
                requestId: null, userId,
                AiInteractionType.request_refine,
                SonnetModel, prompt, raw,
                confidence: null, (int)sw.ElapsedMilliseconds);

            var refinementJson = JsonSerializer.Serialize(new
            {
                original = dto.RawDescription,
                answers = allAnswers,
                generatedAt = DateTime.UtcNow
            }, new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            try
            {
                var parsed = ParseJson<RefineResultDto>(raw);

                var matchedCategory = dto.Categories
                    .FirstOrDefault(c => c.StartsWith($"{parsed.SuggestedCategoryId}:"));

                parsed.SuggestedCategoryName =
                    matchedCategory?.Split(':').ElementAtOrDefault(1);
                parsed.SuggestedCategoryId =
                    parsed.SuggestedCategoryId is > 0 ? parsed.SuggestedCategoryId : null;
                parsed.RefinementJson = refinementJson;

                return parsed;
            }
            catch
            {
                return new RefineResultDto
                {
                    RefinedDescription = dto.RawDescription,
                    AiSummary = dto.RawDescription,
                    SuggestedUrgency = "medium",
                    RefinementJson = refinementJson
                };
            }
        }

        // ── RAG helpers ──────────────────────────────────────────────────

        /// <summary>
        /// Detects which category the description most likely belongs to
        /// by matching Arabic keywords, then fetches completed requests in
        /// that category and extracts their Q&A patterns.
        /// Returns empty list when no history exists (graceful degradation).
        /// </summary>
        private async Task<List<string>> BuildFollowUpPatternsAsync(
            string rawDescription,
            IReadOnlyList<string> categoryStrings)
        {
            var categoryId = DetectCategoryId(rawDescription, categoryStrings);
            if (categoryId == null) return new List<string>();

            var similar = await _requestRepo
                .GetCompletedByCategoryAsync(categoryId.Value, MaxHistoricalRequests);

            var patterns = new List<string>();

            foreach (var r in similar)
            {
                // Build a pattern string from whatever data is available
                var parts = new List<string>();

                if (!string.IsNullOrWhiteSpace(r.AiSummary))
                    parts.Add($"وصف: {r.AiSummary}");
                else if (!string.IsNullOrWhiteSpace(r.Description))
                    parts.Add($"وصف: {r.Description[..Math.Min(80, r.Description.Length)]}");

                // Extract follow-up questions from AiRefinementJson if present
                if (!string.IsNullOrWhiteSpace(r.AiRefinementJson))
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(r.AiRefinementJson);
                        if (doc.RootElement.TryGetProperty("answers", out var answersEl))
                        {
                            var answers = answersEl.EnumerateArray()
                                .Select(a => a.GetString())
                                .Where(a => !string.IsNullOrWhiteSpace(a))
                                .Take(3)
                                .ToList();

                            if (answers.Any())
                                parts.Add($"إجابات سابقة: {string.Join(" / ", answers)}");
                        }
                    }
                    catch { /* malformed JSON — skip gracefully */ }
                }

                if (parts.Any())
                    patterns.Add(string.Join(" | ", parts));
            }

            return patterns;
        }

        /// <summary>
        /// Fetches completed requests in the detected category and returns
        /// their best available description (AiSummary preferred, then
        /// raw Description) as examples for the refinement prompt.
        /// </summary>
        private async Task<List<string>> BuildRefineExamplesAsync(
            IReadOnlyList<string> categoryStrings)
        {
            // Take the first category from the list as a best-guess
            // (at this point the controller has already populated categories)
            var categoryId = ParseFirstCategoryId(categoryStrings);
            if (categoryId == null) return new List<string>();

            var similar = await _requestRepo
                .GetCompletedByCategoryAsync(categoryId.Value, MaxHistoricalRequests);

            return similar
                .Select(r =>
                    !string.IsNullOrWhiteSpace(r.AiSummary) ? r.AiSummary :
                    !string.IsNullOrWhiteSpace(r.Description)
                        ? r.Description[..Math.Min(120, r.Description.Length)]
                        : null)
                .Where(d => d != null)
                .Select(d => d!)
                .Distinct()
                .Take(5)
                .ToList();
        }

        /// <summary>
        /// Simple keyword-based category detector.
        /// Matches Arabic keywords in the description against category names
        /// from the categories list (format: "id:nameAr").
        /// Returns the first matching category id, or null if no match.
        /// This avoids an extra LLM call for category detection.
        /// </summary>
        private static int? DetectCategoryId(
            string description,
            IReadOnlyList<string> categoryStrings)
        {
            if (string.IsNullOrWhiteSpace(description)
                || categoryStrings.Count == 0)
                return null;

            var desc = description.ToLower();

            // Map common Arabic repair keywords to category name substrings
            var keywordMap = new Dictionary<string, string[]>
            {
                { "سباكة",      new[] { "تسريب", "ماء", "مياه", "أنبوب", "حنفية", "صرف" } },
                { "كهرباء",     new[] { "كهرباء", "تيار", "قاطع", "سلك", "شرارة", "مفتاح" } },
                { "تكييف",      new[] { "تكييف", "مكيف", "بارد", "تبريد", "فريون" } },
                { "نجارة",      new[] { "باب", "شباك", "خشب", "دولاب", "سرير", "أثاث" } },
                { "أجهزة",      new[] { "ثلاجة", "غسالة", "تلفزيون", "فرن", "جهاز" } },
                { "دهانات",     new[] { "دهان", "طلاء", "جدار", "سقف", "رسم" } },
                { "سخانات",     new[] { "سخان", "ماء ساخن", "boiler" } },
            };

            foreach (var (categoryKeyword, descKeywords) in keywordMap)
            {
                if (!descKeywords.Any(k => desc.Contains(k)))
                    continue;

                // Find the matching category id from the list
                var match = categoryStrings.FirstOrDefault(c =>
                    c.Contains(categoryKeyword, StringComparison.OrdinalIgnoreCase));

                if (match != null && int.TryParse(match.Split(':')[0], out var id))
                    return id;
            }

            // No keyword match — fall back to first category in list
            return ParseFirstCategoryId(categoryStrings);
        }

        private static int? ParseFirstCategoryId(IReadOnlyList<string> categoryStrings)
        {
            if (categoryStrings.Count == 0) return null;
            var first = categoryStrings[0].Split(':')[0];
            return int.TryParse(first, out var id) ? id : null;
        }
    }
}