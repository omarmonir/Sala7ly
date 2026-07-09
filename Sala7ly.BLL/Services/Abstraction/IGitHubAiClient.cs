using Sala7ly.BLL.DTOs.AiDTOs;
using Sala7ly.DAL.Repositories.Abstraction;

namespace Sala7ly.BLL.Services.Abstraction
{
    /// <summary>
    /// Thin abstraction over the underlying LLM provider (GitHub Models).
    /// Lives in the BLL because it is a business-logic dependency, not a
    /// data-access concern — DAL must never depend on an AI provider.
    /// </summary>
    public interface IGitHubAiClient
    {
        Task<string> CompleteAsync(
            string model,
            string userPrompt,
            string? systemPrompt = null,
            int maxTokens = 500,
            List<ChatMsg>? history = null
        );

        Task<string> CompleteWithImageAsync(
            string model,
            string userPrompt,
            string base64Image,
            string mediaType,
            int maxTokens = 500
        );
    }
}