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
        private const string ClientName = "GeminiEmbeddings";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiKey;
        private readonly string _model;

        public GeminiEmbeddingService(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _apiKey = config["AI:Gemini:ApiKey"]
                ?? throw new InvalidOperationException("AI:Gemini:ApiKey is not configured.");
            _model = config["AI:Gemini:EmbeddingModel"] ?? "gemini-embedding-001";
        }

        public async Task<float[]> GetEmbeddingAsync(string text)
        {
            var http = _httpClientFactory.CreateClient(ClientName);
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:embedContent?key={_apiKey}";

            var requestBody = new
            {
                model = $"models/{_model}",
                content = new { parts = new[] { new { text } } }
            };

            var response = await http.PostAsJsonAsync(url, requestBody);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<GeminiEmbeddingResponse>();
            return result!.Embedding.Values;
        }

        public float CosineSimilarity(float[] a, float[] b)
        {
            float dot = 0, magA = 0, magB = 0;
            var len = Math.Min(a.Length, b.Length); // was a.Length only — threw on mismatched vector sizes

            for (var i = 0; i < len; i++)
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

        private sealed class GeminiEmbeddingResponse
        {
            public GeminiEmbedding Embedding { get; set; } = null!;
        }

        private sealed class GeminiEmbedding
        {
            public float[] Values { get; set; } = Array.Empty<float>();
        }
    }
}