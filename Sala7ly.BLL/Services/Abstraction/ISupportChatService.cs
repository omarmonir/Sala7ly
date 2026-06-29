using Sala7ly.BLL.DTOs.AiDTOs;

namespace Sala7ly.BLL.Services.Abstraction
{
    // Sala7ly.BLL/AI/Interfaces/ISupportChatService.cs
    public interface ISupportChatService
    {
        Task<SupportChatResponseDto> ChatAsync(string userId, SupportChatRequestDto dto);
    }
}