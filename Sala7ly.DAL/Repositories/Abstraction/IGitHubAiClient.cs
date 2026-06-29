namespace Sala7ly.DAL.Repositories.Abstraction
{
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
