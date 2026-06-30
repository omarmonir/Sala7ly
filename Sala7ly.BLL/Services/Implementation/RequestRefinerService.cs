using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Sala7ly.BLL.DTOs.AiDTOs;
using Sala7ly.BLL.Services;
using Sala7ly.BLL.Services.Abstraction;

namespace Sala7ly.BLL.Services.Implementation
{
    public class RequestRefinerService : BaseAiService, IRequestRefinerService
    {
        public RequestRefinerService(
            IGitHubAiClient ai,
            IConfiguration config)
            : base(ai, config)
        {
        }

        // ── Step 1: ask one follow-up question ───────────────────
        public async Task<FollowUpDto> AskFollowUpAsync(FollowUpRequestDto dto)
        {
            var answersCount = dto.PreviousAnswers.Count;
            var previousQA = answersCount > 0
                ? string.Join("\n", dto.PreviousAnswers.Select((a, i) => $"إجابة {i + 1}: {a}"))
                : "لا توجد إجابات سابقة.";
            if (dto.PreviousAnswers.Count >= 3)
                return new FollowUpDto { Question = "", IsComplete = true };

            var prompt = PromptBuilder.RequestRefinementFollowUpUser(
                rawDescription: dto.RawDescription,
                categories: string.Join("، ", dto.Categories),
                previousQA: previousQA,
                answersCount: answersCount);

            var result = await CallAsync(
                model: HaikuModel,
                userPrompt: prompt,
                systemPrompt: PromptBuilder.RequestRefinementSystem(),
                maxTokens: 120);

            try
            {
                var parsed = JsonSerializer.Deserialize<FollowUpDto>(result,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return parsed ?? new FollowUpDto { IsComplete = true };
            }
            catch
            {
                return new FollowUpDto { IsComplete = true };
            }
        }

        // ── Step 2: generate refined description + suggestions ───
        public async Task<RefineResultDto> RefineAsync(RefineRequestDto dto, List<string> allAnswers)
        {
            var answersBlock = allAnswers.Count > 0
                ? string.Join("\n", allAnswers.Select((a, i) => $"إجابة {i + 1}: {a}"))
                : "";

            var prompt = PromptBuilder.RequestRefinementUser(
                rawDescription: dto.RawDescription,
                categories: string.Join("، ", dto.Categories));

            var raw = await CallAsync(
                model: HaikuModel,
                userPrompt: prompt,
                systemPrompt: PromptBuilder.RequestRefinementSystem(),
                maxTokens: 300);

            // store Q&A history as JSON
            var refinementJson = JsonSerializer.Serialize(new
            {
                original = dto.RawDescription,
                answers = allAnswers,
                generatedAt = DateTime.UtcNow
            }, new JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            try
            {
                using var doc = JsonDocument.Parse(raw);
                var root = doc.RootElement;

                var categoryIdRaw = root.GetProperty("suggestedCategoryId").GetInt32();
                var matchedCategory = dto.Categories
                    .FirstOrDefault(c => c.StartsWith($"{categoryIdRaw}:"));

                return new RefineResultDto
                {
                    RefinedDescription = root.GetProperty("refinedDescription").GetString()!,
                    AiSummary = root.GetProperty("aiSummary").GetString()!,
                    SuggestedCategoryId = categoryIdRaw > 0 ? categoryIdRaw : null,
                    SuggestedCategoryName = matchedCategory?.Split(':').ElementAtOrDefault(1),
                    SuggestedUrgency = root.GetProperty("suggestedUrgency").GetString()!,
                    RefinementJson = refinementJson
                };
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

        // No direct HTTP calls; BaseAiService handles GitHub model interaction.
    }
}
