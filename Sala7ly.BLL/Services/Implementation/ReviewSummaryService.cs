using System;
using System.Collections.Generic;
using System.Linq;
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
    public class ReviewSummaryService : BaseAiService, IReviewSummaryService
    {
        private readonly IReviewRepository _reviewRepo;
        private readonly ITechnicianProfileRepository _technicianRepo;
        private readonly IAiInteractionRepository _aiRepo;

        public ReviewSummaryService(
            IGitHubAiClient ai,
            IConfiguration config,
            IReviewRepository reviewRepo,
     ITechnicianProfileRepository technicianRepo,
     IAiInteractionRepository aiRepo)
     : base(ai, config, aiRepo)
        {
            _reviewRepo = reviewRepo;
            _technicianRepo = technicianRepo;
            _aiRepo = aiRepo;
        }

        public async Task SummarizeTechnicianAsync(int technicianId)
        {
            var technician = await _technicianRepo.GetByIdAsync(technicianId)
                ?? throw new KeyNotFoundException($"الفني ذو المعرّف {technicianId} غير موجود.");

            var allReviews = await _reviewRepo.GetByRevieweeIdAsync(technician.UserId);
            var comments = allReviews
                .Where(r => !string.IsNullOrWhiteSpace(r.Comment))
                .Select(r => r.Comment!)
                .ToList();

            if (!comments.Any()) return;

            var prompt = PromptBuilder.ReviewSummaryUser(comments);

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var raw = await CallAsync(HaikuModel, prompt, maxTokens: 400);
            sw.Stop();

            ReviewSummaryResultDto result;
            try
            {
                result = ParseJson<ReviewSummaryResultDto>(raw);
            }
            catch
            {
                await WriteLogAsync(technician.UserId, prompt, raw, sw.ElapsedMilliseconds);
                return;
            }

            technician.ReviewSummary = result.Summary;
            technician.SentimentScore = result.SentimentScore;
            technician.TopStrengths = string.Join("|", result.Strengths ?? new());
            technician.CommonComplaints = string.Join("|", result.Complaints ?? new());
            technician.SummaryUpdatedAt = DateTime.UtcNow;

            _technicianRepo.Update(technician);
            await _technicianRepo.SaveChangesAsync();

            await WriteLogAsync(technician.UserId, prompt, raw, sw.ElapsedMilliseconds, (float)result.SentimentScore);
        }

        public async Task SummarizeAllAsync()
        {
            var technicians = await _technicianRepo.GetAllAsync();
            foreach (var t in technicians)
            {
                try { await SummarizeTechnicianAsync(t.Id); }
                catch { }
            }
        }

        private async Task WriteLogAsync(
            string userId, string prompt, string response, long latencyMs, float confidence = 0f)
        {
            try
            {
                await _aiRepo.AddAsync(new Ai_Interaction
                {
                    UserId = userId,
                    InteractionType = AiInteractionType.chat_summary,
                    ModelUsed = HaikuModel,
                    PromptSnapshot = prompt,
                    ResponseSnapshot = response,
                    ConfidenceScore = confidence,
                    LatencyMs = (int)latencyMs,
                    CreatedOn = DateTime.UtcNow
                });
                await _aiRepo.SaveChangesAsync();
            }
            catch { }
        }
    }
}
