using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Sala7ly.BLL.Services.Abstraction;
using Sala7ly.DAL.Entities;

namespace Sala7ly.BLL.Services.Implementation
{
    public class GeminiEmbeddingService : IEmbeddingService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;
        private readonly string _model;

        public GeminiEmbeddingService(IHttpClientFactory httpFactory, IConfiguration config)
        {
            _http = httpFactory.CreateClient("GeminiEmbedding");
            _apiKey = config["AI:Gemini:ApiKey"]!;
            _model = config["AI:Gemini:EmbeddingModel"] ?? "gemini-embedding-001";
        }

        public async Task<float[]> GetEmbeddingAsync(string text)
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:embedText?key={_apiKey}";

            var requestBody = new
            {
                text = text
            };

            var response = await _http.PostAsJsonAsync(url, requestBody);
            response.EnsureSuccessStatusCode();

            var result = await response.Content
                .ReadFromJsonAsync<GeminiEmbeddingResponse>();

            return result?.Embedding?.Values ?? Array.Empty<float>();
        }

        public float CosineSimilarity(float[] a, float[] b)
        {
            float dot = 0, magA = 0, magB = 0;

            for (int i = 0; i < a.Length; i++)
            {
                dot += a[i] * b[i];
                magA += a[i] * a[i];
                magB += b[i] * b[i];
            }

            if (magA == 0 || magB == 0) return 0;
            return dot / (MathF.Sqrt(magA) * MathF.Sqrt(magB));
        }

        public string BuildTechnicianText(TechnicianProfile t)
        {
            var categories = t.Categories?
                .Select(c => c.Category?.NameAr ?? "")
                .ToList() ?? new List<string>();

            return $"""
                مهارات: {string.Join("، ", categories)}.
                خبرة: {t.ExperienceYears} سنوات.
                نبذة: {t.Bio}.
                وظائف مكتملة: {t.CompletedJobs}.
                تقييم: {t.OverallRating}/5.
                """;
        }

        // ── Response Models ───────────────────────────────────
        private class GeminiEmbeddingResponse
        {
            public GeminiEmbedding Embedding { get; set; } = null!;
        }

        private class GeminiEmbedding
        {
            public float[] Values { get; set; } = Array.Empty<float>();
        }
    }
}