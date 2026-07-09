using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Sala7ly.BLL.DTOs.AiDTOs;
using Sala7ly.BLL.Services.Abstraction;
using Sala7ly.DAL.Entities;
using Sala7ly.DAL.Enums;
using Sala7ly.DAL.Repositories.Abstraction;

namespace Sala7ly.BLL.Services.Implementation
{
    /// <summary>
    /// Shared base for every service that talks to the LLM provider.
    /// Centralises model resolution, JSON parsing, and AI-interaction audit
    /// logging so concrete AI services stay thin and never re-implement
    /// this plumbing (previously duplicated across 4 different services).
    /// </summary>
    public abstract class BaseAiService
    {
        private readonly IGitHubAiClient _ai;
        private readonly IAiInteractionRepository _aiInteractionRepo;
        protected readonly IConfiguration _config;

        protected string HaikuModel => _config["AI:GitHub:HaikuModel"]!;
        protected string SonnetModel => _config["AI:GitHub:SonnetModel"]!;
        protected string OpusModel => _config["AI:GitHub:OpusModel"]!;

        protected BaseAiService(
            IGitHubAiClient ai,
            IConfiguration config,
            IAiInteractionRepository aiInteractionRepo)
        {
            _ai = ai;
            _config = config;
            _aiInteractionRepo = aiInteractionRepo;
        }

        protected async Task<string> CallAsync(
            string model,
            string userPrompt,
            string? systemPrompt = null,
            int maxTokens = 500,
            List<ChatMsg>? history = null)
        {
            return await _ai.CompleteAsync(model, userPrompt, systemPrompt, maxTokens, history);
        }

        protected async Task<string> CallWithImageAsync(
            string model,
            string userPrompt,
            string base64Image,
            string mediaType,
            int maxTokens = 500)
        {
            return await _ai.CompleteWithImageAsync(model, userPrompt, base64Image, mediaType, maxTokens);
        }

        protected static T ParseJson<T>(string raw)
        {
            var clean = raw
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();

            return JsonSerializer.Deserialize<T>(clean,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        /// <summary>
        /// Persists an audit row for a single LLM call. Best-effort: a logging
        /// failure must never fail the business operation that triggered it.
        /// </summary>
        protected async Task LogInteractionAsync(
            int? requestId,
            string? userId,
            AiInteractionType type,
            string model,
            string promptSnapshot,
            string responseSnapshot,
            float? confidence,
            int latencyMs)
        {
            try
            {
                await _aiInteractionRepo.AddAsync(new Ai_Interaction
                {
                    RequestId = requestId,
                    UserId = userId ?? string.Empty,
                    InteractionType = type,
                    ModelUsed = model,
                    PromptSnapshot = promptSnapshot,
                    ResponseSnapshot = responseSnapshot,
                    ConfidenceScore = confidence,
                    LatencyMs = latencyMs,
                    CreatedOn = DateTime.UtcNow
                });
                await _aiInteractionRepo.SaveChangesAsync();
            }
            catch
            {
                // Audit logging is best-effort; never let it break the caller.
            }
        }
    }
}