using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Sala7ly.BLL.DTOs.AiDTOs;
using Sala7ly.BLL.Services.Abstraction;
using Sala7ly.DAL.Entities;
using Sala7ly.DAL.Enums;
using Sala7ly.DAL.Repositories.Abstraction;

namespace Sala7ly.BLL.Services.Implementation
{
    public partial class TechnicianMatchingService : BaseAiService, ITechnicianMatchingService
    {
        private const float SemanticSimilarityThreshold = 0.40f;
        private const int ShortlistSize = 20;

        private readonly IEmbeddingService _embedding;
        private readonly ITechnicianProfileRepository _technicianRepo;
        private readonly IServiceRequestRepository _requestRepo;

        public TechnicianMatchingService(
            IGitHubAiClient ai,
            IConfiguration config,
            IAiInteractionRepository aiInteractionRepo,
            IEmbeddingService embedding,
            ITechnicianProfileRepository technicianRepo,
            IServiceRequestRepository requestRepo)
            : base(ai, config, aiInteractionRepo)
        {
            _embedding = embedding;
            _technicianRepo = technicianRepo;
            _requestRepo = requestRepo;
        }

        public async Task<List<TechnicianMatchDto>> FindMatchesAsync(int requestId, int topN = 10)
        {
            var request = await _requestRepo.GetByIdWithDetailsAsync(requestId)
                ?? throw new KeyNotFoundException("الطلب غير موجود."); // was a plain Exception

            var requestText = BuildRequestText(request);
            var requestVector = await _embedding.GetEmbeddingAsync(requestText);

            var technicians = await _technicianRepo.GetApprovedWithEmbeddingsAsync();
            if (!technicians.Any())
                return new List<TechnicianMatchDto>();

            // Cosine similarity via the single shared implementation on
            // IEmbeddingService (this class used to duplicate the formula).
            var shortlist = technicians
                .Where(t => t.EmbeddingVector != null)
                .Select(t => new
                {
                    Technician = t,
                    SemanticScore = _embedding.CosineSimilarity(requestVector, t.EmbeddingVector!)
                })
                .Where(x => x.SemanticScore > SemanticSimilarityThreshold)
                .OrderByDescending(x => x.SemanticScore)
                .Take(ShortlistSize)
                .ToList();

            if (!shortlist.Any())
                return new List<TechnicianMatchDto>();

            var rerankScores = await RerankWithLlmAsync(
                requestId,
                request.Profile?.UserId,
                requestText,
                shortlist.Select(s => _embedding.BuildTechnicianText(s.Technician)).ToList());

            var final = new List<TechnicianMatchDto>();
            for (var i = 0; i < shortlist.Count && i < rerankScores.Count; i++)
            {
                var item = shortlist[i];
                var llmScore = rerankScores[i];
                var blended = BlendScore(
                    semantic: llmScore,
                    rating: item.Technician.OverallRating / 5.0,
                    distance: GetDistanceScore(request, item.Technician),
                    jobs: Math.Min(item.Technician.CompletedJobs / 100.0, 1.0));

                final.Add(new TechnicianMatchDto
                {
                    TechnicianId = item.Technician.Id,
                    UserId = item.Technician.UserId,
                    Name = item.Technician.User.Name,
                    Bio = item.Technician.Bio,
                    ExperienceYears = item.Technician.ExperienceYears,
                    OverallRating = item.Technician.OverallRating,
                    TotalReviews = item.Technician.TotalReviews,
                    CompletedJobs = item.Technician.CompletedJobs,
                    CategoryNames = item.Technician.Categories.Select(c => c.Category.NameAr).ToList(),

                    FinalScore = blended,
                    MatchReason = GenerateMatchReason(llmScore)
                });
            }

            return final
                .OrderByDescending(x => x.FinalScore)
                .Take(topN)
                .ToList();
        }

        /// <summary>
        /// Sends the request + shortlisted technician profiles to the LLM
        /// and asks for a 0.0–1.0 relevance score per technician (replaces
        /// the Cohere Rerank API). Falls back to a uniform score, preserving
        /// the cosine-similarity order, if the LLM call or parse fails.
        /// </summary>
        private async Task<List<double>> RerankWithLlmAsync(
            int requestId,
            string? userId,
            string requestText,
            List<string> technicianTexts)
        {
            var prompt = PromptBuilder.TechnicianRerankUser(requestText, technicianTexts);
            var sw = Stopwatch.StartNew();

            try
            {
                var raw = await CallAsync(HaikuModel, prompt, maxTokens: 400);
                sw.Stop();

                var parsed = ParseJson<List<LlmRerankItem>>(raw);

                await LogInteractionAsync(
                    requestId, userId, AiInteractionType.matching,
                    HaikuModel, prompt, raw, confidence: null, (int)sw.ElapsedMilliseconds);

                return parsed
                    .OrderBy(x => x.Index)
                    .Select(x => Math.Clamp(x.Score, 0.0, 1.0))
                    .ToList();
            }
            catch
            {
                return Enumerable.Repeat(0.5, technicianTexts.Count).ToList();
            }
        }

        private static double BlendScore(double semantic, double rating, double distance, double jobs)
            => (semantic * 0.40)
             + (rating * 0.30)
             + (distance * 0.20)
             + (jobs * 0.10);

        /// <summary>
        /// Returns a 0-1 proximity score.
        /// TODO: replace with a real haversine distance calculation once
        /// Address.Latitude / Address.Longitude are added to the schema.
        /// </summary>
        private static double GetDistanceScore(ServiceRequest request, TechnicianProfile technician) => 0.5;

        private static string GenerateMatchReason(double score) => score switch
        {
            >= 0.80 => "مطابقة ممتازة بناءً على الخبرة والتقييمات",
            >= 0.60 => "مطابقة جيدة مع مهارات ذات صلة",
            >= 0.40 => "مطابقة مقبولة – يُنصح بمراجعة ملف الفني",
            _ => "مطابقة جزئية"
        };

        private static string BuildRequestText(ServiceRequest r)
            => $"{r.Title} {r.Description} {r.AiSummary}".Trim();

        private sealed class LlmRerankItem
        {
            public int Index { get; set; }
            public double Score { get; set; }
        }
    }
}