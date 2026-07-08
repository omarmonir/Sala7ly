using Sala7ly.BLL.DTOs.AiSupportDTOs;

namespace Sala7ly.BLL.Services.Abstraction
{
    public interface IAiSupportService
    {
        Task<AiSupportResponseDto> AskAsync(string userId, AiSupportRequestDto dto);
    }
}