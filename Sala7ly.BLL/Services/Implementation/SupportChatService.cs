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
    public class SupportChatService : BaseAiService, ISupportChatService
    {
        public SupportChatService(
            IGitHubAiClient ai,
            IConfiguration config,
            IAiInteractionRepository aiInteractionRepo)
            : base(ai, config, aiInteractionRepo)
        {
        }

        public async Task<SupportChatResponseDto> ChatAsync(string userId, SupportChatRequestDto dto)
        {
            var sessionId = string.IsNullOrWhiteSpace(dto.SessionId)
                ? Guid.NewGuid().ToString()
                : dto.SessionId!;

            var history = dto.History?
                .Select(h => new ChatMsg { Role = h.Role, Content = h.Content })
                .ToList();

            var raw = await CallAsync(
                model: HaikuModel,
                userPrompt: dto.Message,
                systemPrompt: PromptBuilder.SupportChatSystem(),
                maxTokens: 250,
                history: history);

            SupportChatResponseDto result;
            try
            {
                result = ParseJson<SupportChatResponseDto>(raw);
            }
            catch
            {
                result = new SupportChatResponseDto
                {
                    Answer = raw.Trim(),
                    NeedsHuman = false,
                    SessionId = sessionId
                };
            }

            if (string.IsNullOrWhiteSpace(result.SessionId))
                result.SessionId = sessionId;

            return result;
        }
    }
}
