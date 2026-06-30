using System.ClientModel;
using Azure;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;
using Sala7ly.BLL.Services.Abstraction;

namespace Sala7ly.BLL.Services.Implementation
{
    public class GitHubAiClient : IGitHubAiClient
    {
        private readonly OpenAIClient _openAiClient;

        public GitHubAiClient(IConfiguration config)
        {
            var token = config["AI:GitHub:Token"]!;
            var endpoint = config["AI:GitHub:Endpoint"]!;

            _openAiClient = new OpenAIClient(
                new ApiKeyCredential(token),
                new OpenAIClientOptions
                {
                    Endpoint = new Uri(endpoint)
                }
            );
        }

        public async Task<string> CompleteAsync(
            string model,
            string userPrompt,
            string? systemPrompt = null,
            int maxTokens = 500,
            List<ChatMsg>? history = null)
        {
            int maxRetries = 3;
            int delay = 2000;

            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    var messages = new List<ChatMessage>();

                    if (!string.IsNullOrWhiteSpace(systemPrompt))
                        messages.Add(new SystemChatMessage(systemPrompt));

                    if (history != null)
                        foreach (var h in history)
                            messages.Add(h.Role == "user"
                                ? new UserChatMessage(h.Content)
                                : (ChatMessage)new AssistantChatMessage(h.Content));

                    messages.Add(new UserChatMessage(userPrompt));

                    var client = _openAiClient.GetChatClient(model);
                    var response = await client.CompleteChatAsync(
                        messages,
                        new ChatCompletionOptions { MaxOutputTokenCount = maxTokens }
                    );

                    return response.Value.Content[0].Text;
                }
                catch (Exception ex) when (ex.Message.Contains("429"))
                {
                    if (attempt == maxRetries - 1) throw;
                    await Task.Delay(delay);
                    delay *= 2;
                }
            }

            throw new Exception("AI service unavailable after retries.");
        }

        public async Task<string> CompleteWithImageAsync(
            string model,
            string userPrompt,
            string base64Image,
            string mediaType,
            int maxTokens = 500)
        {
            var imageBytes = Convert.FromBase64String(base64Image);
            var imagePart = ChatMessageContentPart.CreateImagePart(
                                 BinaryData.FromBytes(imageBytes), mediaType);
            var textPart = ChatMessageContentPart.CreateTextPart(userPrompt);

            var messages = new List<ChatMessage>
            {
                new UserChatMessage(imagePart, textPart)
            };

            var client = _openAiClient.GetChatClient(model);
            var response = await client.CompleteChatAsync(
                messages,
                new ChatCompletionOptions { MaxOutputTokenCount = maxTokens }
            );

            return response.Value.Content[0].Text;
        }
    }
}
