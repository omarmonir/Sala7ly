using Microsoft.AspNetCore.Http;
using Sala7ly.BLL.DTOs.ChatDTOs;

namespace Sala7ly.BLL.Services.Abstraction
{
    public interface IChatService
    {
        Task<ChatMessageResponseDto> SaveMessageAsync(string senderId, SendMessageDto dto, IFormFileCollection? files);

        Task<List<ChatMessageResponseDto>> GetHistoryAsync(string userId, int requestId, int page = 1, int pageSize = 30);

        Task MarkAsReadAsync(int requestId, string userId);

        Task<bool> CanAccessRequestAsync(string userId, int requestId);
        Task<List<ConversationSummaryDto>> GetConversationsAsync(string userId);
    }
}
