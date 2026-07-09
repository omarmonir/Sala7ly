using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Sala7ly.BLL.DTOs.AiDTOs;
using Sala7ly.BLL.Services.Abstraction;
using Sala7ly.DAL.Entities;
using Sala7ly.DAL.Enums;
using Sala7ly.DAL.Repositories.Abstraction;

namespace Sala7ly.BLL.Services.Implementation
{
    public class DisputeAnalysisService : BaseAiService, IDisputeAnalysisService
    {
        private readonly IAiInteractionRepository _aiRepo;

        public DisputeAnalysisService(
            IGitHubAiClient ai,
            IConfiguration config,
     IAiInteractionRepository aiRepo)
     : base(ai, config, aiRepo)
        {
            _aiRepo = aiRepo;
        }

        // Called by IDisputeAnalysisService.AnalyzeAsync(int disputeId) —
        // wire up IDisputeRepository here once it exists.
        public async Task<DisputeAnalysisDto> AnalyzeAsync(int disputeId)
        {
            throw new NotImplementedException(
                $"DisputeAnalysisService.AnalyzeAsync({disputeId}) requires " +
                "IDisputeRepository to be injected. Use the overload that accepts plain strings.");
        }

        // Used directly by AiController until IDisputeRepository is implemented.
        public async Task<DisputeAnalysisDto> AnalyzeAsync(
            int? disputeId,
            string customerClaim,
            string technicianClaim,
            string requestDetails)
        {
            var prompt = PromptBuilder.DisputeAnalysisUser(customerClaim, technicianClaim, requestDetails);

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var raw = await CallAsync(SonnetModel, prompt, maxTokens: 600);
            sw.Stop();

            DisputeAnalysisDto result;
            try
            {
                result = ParseJson<DisputeAnalysisDto>(raw);
            }
            catch (Exception ex)
            {
                await WriteLogAsync(disputeId, prompt, raw, sw.ElapsedMilliseconds);
                throw new InvalidOperationException($"Failed to parse AI dispute analysis: {ex.Message}", ex);
            }

            await WriteLogAsync(disputeId, prompt, raw, sw.ElapsedMilliseconds);
            return result;
        }

        private async Task WriteLogAsync(int? disputeId, string prompt, string response, long latencyMs)
        {
            try
            {
                await _aiRepo.AddAsync(new Ai_Interaction
                {
                    RequestId = disputeId,
                    UserId = string.Empty,
                    InteractionType = AiInteractionType.categorization,
                    ModelUsed = SonnetModel,
                    PromptSnapshot = prompt,
                    ResponseSnapshot = response,
                    LatencyMs = (int)latencyMs,
                    CreatedOn = DateTime.UtcNow
                });
                await _aiRepo.SaveChangesAsync();
            }
            catch { }
        }
    }
}
