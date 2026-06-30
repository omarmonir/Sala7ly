using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Sala7ly.BLL.DTOs.AiDTOs;
using Sala7ly.BLL.Services.Abstraction;
using Sala7ly.DAL.Repositories.Abstraction;

namespace Sala7ly.BLL.Services.Implementation
{
    public class ReviewSummaryService : BaseAiService, IReviewSummaryService
    {
        private readonly IReviewRepository _reviewRepo;
        private readonly ITechnicianProfileRepository _technicianRepo;

        public ReviewSummaryService(
            IGitHubAiClient ai,
            IConfiguration config,
            IReviewRepository reviewRepo,
            ITechnicianProfileRepository technicianRepo)
            : base(ai, config)
        {
            _reviewRepo = reviewRepo;
            _technicianRepo = technicianRepo;
        }

        public async Task SummarizeAllAsync()
        {
            var technicians = await _technicianRepo.GetAllAsync();
            foreach (var technician in technicians)
            {
                if (technician.TotalReviews <= 0)
                    continue;

                try
                {
                    await SummarizeTechnicianAsync(technician.Id);
                }
                catch
                {
                    // best-effort summary generation
                }
            }
        }

        public async Task SummarizeTechnicianAsync(int technicianId)
        {
            var technician = await _technicianRepo.GetByIdAsync(technicianId);
            if (technician == null)
                throw new KeyNotFoundException("الفني غير موجود.");

            var reviews = await _reviewRepo.GetByRevieweeIdAsync(technician.UserId);
            if (!reviews.Any())
            {
                technician.ReviewSummary = "لا توجد تقييمات كافية بعد.";
                technician.SentimentScore = null;
                technician.TopStrengths = null;
                technician.CommonComplaints = null;
                technician.SummaryUpdatedAt = DateTime.UtcNow;
                _technicianRepo.Update(technician);
                await _technicianRepo.SaveChangesAsync();
                return;
            }

            var reviewLines = reviews
                .Select(r => $"{r.CreatedAt:yyyy-MM-dd} | {r.OverallScore:F1}/5 | {r.Comment}")
                .ToList();

            var prompt = PromptBuilder.ReviewSummaryUser(reviewLines);
            var raw = await CallAsync(HaikuModel, prompt, systemPrompt: null, maxTokens: 250);

            ReviewSummaryResultDto result;
            try
            {
                result = ParseJson<ReviewSummaryResultDto>(raw);
            }
            catch
            {
                result = new ReviewSummaryResultDto
                {
                    Summary = "تعذر توليد ملخص التقييمات في الوقت الحالي.",
                    Strengths = new List<string>(),
                    Complaints = new List<string>(),
                    SentimentScore = 0.5
                };
            }

            technician.ReviewSummary = result.Summary;
            technician.TopStrengths = result.Strengths is { Count: > 0 }
                ? string.Join("، ", result.Strengths)
                : null;
            technician.CommonComplaints = result.Complaints is { Count: > 0 }
                ? string.Join("، ", result.Complaints)
                : null;
            technician.SentimentScore = Math.Clamp(result.SentimentScore, 0.0, 1.0);
            technician.SummaryUpdatedAt = DateTime.UtcNow;

            _technicianRepo.Update(technician);
            await _technicianRepo.SaveChangesAsync();
        }
    }
}
