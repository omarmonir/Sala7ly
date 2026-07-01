using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Sala7ly.BLL.DTOs.AiDTOs;
using Sala7ly.BLL.Services.Abstraction;
using Sala7ly.DAL.Enums;
using Sala7ly.DAL.Repositories.Abstraction;

namespace Sala7ly.BLL.Services.Implementation
{
    public class RequestRefinerService : BaseAiService, IRequestRefinerService
    {
        private const int MaxFollowUpQuestions = 3;

        public RequestRefinerService(
            IGitHubAiClient ai,
            IConfiguration config,
            IAiInteractionRepository aiInteractionRepo)
            : base(ai, config, aiInteractionRepo)
        {
        }

        // ── Step 1: ask one follow-up question ───────────────────
        public async Task<FollowUpDto> AskFollowUpAsync(FollowUpRequestDto dto, string? userId = null)
        {
            if (dto.PreviousAnswers.Count >= MaxFollowUpQuestions)
                return new FollowUpDto { Question = "", IsComplete = true };

            var prompt = PromptBuilder.RequestFollowUpUser(dto.RawDescription, dto.Categories, dto.PreviousAnswers);

            var sw = Stopwatch.StartNew();
            var raw = await CallAsync(HaikuModel, prompt, maxTokens: 200);
            sw.Stop();

            await LogInteractionAsync(
                requestId: null, userId, AiInteractionType.request_refine,
                HaikuModel, prompt, raw, confidence: null, (int)sw.ElapsedMilliseconds);

            try
            {
                return ParseJson<FollowUpDto>(raw);
            }
            catch
            {
                return new FollowUpDto { Question = "", IsComplete = true };
            }
        }

        // ── Step 2: generate refined description + suggestions ───
        public async Task<RefineResultDto> RefineAsync(RefineRequestDto dto, List<string> allAnswers, string? userId = null)
        {
            var prompt = PromptBuilder.RequestRefineUser(dto.RawDescription, dto.Categories, allAnswers);

            var sw = Stopwatch.StartNew();
             
            var raw = await CallAsync(HaikuModel, prompt, maxTokens: 200);
            sw.Stop();

            Console.WriteLine("========== AI RESPONSE ==========");
            Console.WriteLine(raw);
            Console.WriteLine("=================================");

            await LogInteractionAsync(
                requestId: null, userId, AiInteractionType.request_refine,
                SonnetModel, prompt, raw, confidence: null, (int)sw.ElapsedMilliseconds);

            // store Q&A history as JSON regardless of parse outcome
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

                parsed.SuggestedCategoryName = matchedCategory?.Split(':').ElementAtOrDefault(1);
                parsed.SuggestedCategoryId = parsed.SuggestedCategoryId is > 0 ? parsed.SuggestedCategoryId : null;
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
    }
}