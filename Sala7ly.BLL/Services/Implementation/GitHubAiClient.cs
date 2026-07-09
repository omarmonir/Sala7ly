using System.ClientModel;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;
using Sala7ly.BLL.DTOs.AiDTOs;
using Sala7ly.BLL.Services.Abstraction;

namespace Sala7ly.BLL.Services.Implementation
{
    public class GitHubAiClient : IGitHubAiClient
    {
        private readonly OpenAIClient _openAiClient;
        private const int MaxRetries = 3;
        private const int InitialRetryDelayMs = 2000;

        public GitHubAiClient(IConfiguration config)
        {
            var token = config["AI:GitHub:Token"]
                ?? throw new InvalidOperationException("AI:GitHub:Token is not configured.");
            var endpoint = config["AI:GitHub:Endpoint"]
                ?? throw new InvalidOperationException("AI:GitHub:Endpoint is not configured.");

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
            var delay = InitialRetryDelayMs;

            for (var attempt = 0; attempt < MaxRetries; attempt++)
            {
                try
                {
                    var messages = BuildMessages(systemPrompt, history, userPrompt);
                    var client = _openAiClient.GetChatClient(model);
                    var response = await client.CompleteChatAsync(
                        messages,
                        new ChatCompletionOptions { MaxOutputTokenCount = maxTokens }
                    );

                    return response.Value.Content[0].Text;
                }
                catch (Exception ex) when (IsRateLimitError(ex) && attempt < MaxRetries - 1)
                {
                    await Task.Delay(delay);
                    delay *= 2;
                }
            }

            throw new InvalidOperationException("AI service unavailable after retries.");
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

        private static List<ChatMessage> BuildMessages(
            string? systemPrompt, List<ChatMsg>? history, string userPrompt)
        {
            var messages = new List<ChatMessage>();

            if (!string.IsNullOrWhiteSpace(systemPrompt))
                messages.Add(new SystemChatMessage(systemPrompt));

            if (history != null)
            {
                foreach (var h in history)
                {
                    if (h.Role == "user")
                        messages.Add(new UserChatMessage(h.Content));
                    else
                        messages.Add(new AssistantChatMessage(h.Content));
                }
            }

            messages.Add(new UserChatMessage(userPrompt));
            return messages;
        }

        // GitHub Models / Azure AI Inference surfaces rate limiting as HTTP 429.
        // Checking the typed SDK exception + status code is far more reliable
        // than string-matching ex.Message (the previous approach).
        private static bool IsRateLimitError(Exception ex)
            => ex is ClientResultException cre && cre.Status == 429;
    }
}