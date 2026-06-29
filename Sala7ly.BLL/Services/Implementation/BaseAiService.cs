using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Sala7ly.DAL.Repositories.Abstraction;

namespace Sala7ly.BLL.Services.Implementation
{
    public abstract class BaseAiService
    {
        protected readonly IGitHubAiClient _ai;
        protected readonly IConfiguration _config;

        protected string HaikuModel => _config["AI:GitHub:HaikuModel"]!;
        protected string SonnetModel => _config["AI:GitHub:SonnetModel"]!;
        protected string OpusModel => _config["AI:GitHub:OpusModel"]!;

        protected BaseAiService(IGitHubAiClient ai, IConfiguration config)
        {
            _ai = ai;
            _config = config;
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

        protected T ParseJson<T>(string raw)
        {
            var clean = raw
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();

            return JsonSerializer.Deserialize<T>(clean,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }
    }


}
