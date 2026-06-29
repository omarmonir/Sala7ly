using Microsoft.Extensions.Configuration;
using Sala7ly.BLL.DTOs.AiDTOs;
using Sala7ly.BLL.Services.Abstraction;
using Sala7ly.DAL.Entities;
using Sala7ly.DAL.Repositories.Abstraction;

namespace Sala7ly.BLL.Services.Implementation
{

    public class MatchingService : BaseAiService, IMatchingService
    {
        private readonly IEmbeddingService _embedding;
        private readonly ITechnicianProfileRepository _technicianRepo;
        private readonly IServiceRequestRepository _requestRepo;

        public MatchingService(
            IGitHubAiClient ai,
            IConfiguration config,
            IEmbeddingService embedding,
            ITechnicianProfileRepository technicianRepo,
            IServiceRequestRepository requestRepo)
            : base(ai, config)
        {
            _embedding = embedding;
            _technicianRepo = technicianRepo;
            _requestRepo = requestRepo;
        }

        // ─────────────────────────────────────────────────────────────────────
        public async Task<List<TechnicianMatchDto>> FindMatchesAsync(
            int requestId,
            int topN = 10)
        {
            // ── 1. Load request ───────────────────────────────────────────────
            var request = await _requestRepo.GetByIdWithDetailsAsync(requestId)
                ?? throw new Exception("الطلب غير موجود.");

            var requestText = BuildRequestText(request);

            // ── 2. Embed request (Gemini) ─────────────────────────────────────
            var requestVector = await _embedding.GetEmbeddingAsync(requestText);

            // ── 3. Load approved technicians that have stored embedding vectors ─
            var technicians = await _technicianRepo.GetApprovedWithEmbeddingsAsync();

            if (!technicians.Any())
                return new List<TechnicianMatchDto>();

            // ── 4. Cosine similarity – keep top 20 above threshold ────────────
            var shortlist = technicians
                .Where(t => t.EmbeddingVector != null)
                .Select(t => new
                {
                    Technician = t,
                    SemanticScore = CosineSimilarity(requestVector, t.EmbeddingVector!)
                })
                .Where(x => x.SemanticScore > 0.40f)
                .OrderByDescending(x => x.SemanticScore)
                .Take(20)
                .ToList();

            if (!shortlist.Any())
                return new List<TechnicianMatchDto>();

            // ── 5. LLM re-rank via GitHub Models ─────────────────────────────
            //    We ask the model to score each candidate 0-1 for relevance
            //    and return a JSON array.  This replaces the Cohere Rerank API.
            var rerankScores = await RerankWithLlmAsync(requestText, shortlist
                .Select(s => _embedding.BuildTechnicianText(s.Technician))
                .ToList());

            // ── 6. Blend scores & build result ────────────────────────────────
            var final = new List<TechnicianMatchDto>();

            for (int i = 0; i < shortlist.Count && i < rerankScores.Count; i++)
            {
                var item = shortlist[i];
                var llmScore = rerankScores[i];          // 0-1 from LLM
                var blended = BlendScore(
                    semantic: llmScore,
                    rating: item.Technician.OverallRating / 5.0,
                    distance: GetDistanceScore(request, item.Technician),
                    jobs: Math.Min(item.Technician.CompletedJobs / 100.0, 1.0));

                final.Add(new TechnicianMatchDto
                {
                    Technician = item.Technician,
                    FinalScore = blended,
                    MatchReason = GenerateMatchReason(llmScore)
                });
            }

            return final
                .OrderByDescending(x => x.FinalScore)
                .Take(topN)
                .ToList();
        }

        // ── Re-rank helpers ───────────────────────────────────────────────────

        /// <summary>
        /// Sends the request description + technician profiles to the LLM and
        /// asks for a relevance score (0.0–1.0) for each technician.
        /// Returns scores in the same order as <paramref name="technicianTexts"/>.
        /// Falls back to uniform 0.5 if the LLM response cannot be parsed.
        /// </summary>
        private async Task<List<double>> RerankWithLlmAsync(
            string requestText,
            List<string> technicianTexts)
        {
            // Build a compact numbered list so the prompt stays within token budget
            var numbered = string.Join("\n", technicianTexts
                .Select((t, i) => $"[{i}] {t}"));

            var prompt = $$"""
                أنت نظام مطابقة خدمات. قيّم مدى ملاءمة كل فني للطلب التالي.
                أعد مصفوفة JSON فقط بدون أي نص آخر.
                الصيغة: [{"index":0,"score":0.85}, {"index":1,"score":0.42}, ...]
                النتيجة بين 0 (غير مناسب) و 1 (مناسب جداً).

                الطلب:
                {{requestText}}

                الفنيون:
                {{numbered}}
                """;

            try
            {
                var raw = await CallAsync(HaikuModel, prompt, maxTokens: 400);
                var parsed = ParseJson<List<LlmRerankItem>>(raw);

                // Return scores sorted by index to preserve original order
                return parsed
                    .OrderBy(x => x.Index)
                    .Select(x => Math.Clamp(x.Score, 0.0, 1.0))
                    .ToList();
            }
            catch
            {
                // Graceful degradation: uniform score so cosine order is preserved
                return Enumerable.Repeat(0.5, technicianTexts.Count).ToList();
            }
        }

        // ── Scoring helpers ───────────────────────────────────────────────────

        private double BlendScore(
            double semantic,
            double rating,
            double distance,
            double jobs)
            => (semantic * 0.40)
             + (rating * 0.30)
             + (distance * 0.20)
             + (jobs * 0.10);

        /// <summary>
        /// Returns a 0-1 proximity score.
        /// Currently a stub — replace with a real geo calculation once
        /// Address.Latitude / Address.Longitude are added to the schema.
        /// </summary>
        private double GetDistanceScore(ServiceRequest request, TechnicianProfile technician)
        {
            // TODO: compute haversine distance and normalise to 0-1
            return 0.5;
        }

        private float CosineSimilarity(float[] a, float[] b)
        {
            float dot = 0, magA = 0, magB = 0;
            for (int i = 0; i < Math.Min(a.Length, b.Length); i++)
            {
                dot += a[i] * b[i];
                magA += a[i] * a[i];
                magB += b[i] * b[i];
            }
            if (magA == 0 || magB == 0) return 0;
            return dot / (MathF.Sqrt(magA) * MathF.Sqrt(magB));
        }

        private string GenerateMatchReason(double score) => score switch
        {
            >= 0.80 => "مطابقة ممتازة بناءً على الخبرة والتقييمات",
            >= 0.60 => "مطابقة جيدة مع مهارات ذات صلة",
            >= 0.40 => "مطابقة مقبولة – يُنصح بمراجعة ملف الفني",
            _ => "مطابقة جزئية"
        };

        private string BuildRequestText(ServiceRequest r)
            => $"{r.Title} {r.Description} {r.AiSummary}".Trim();

        // ── DTO for LLM rerank response ───────────────────────────────────────

        private sealed class LlmRerankItem
        {
            public int Index { get; set; }
            public double Score { get; set; }
        }
    }
}