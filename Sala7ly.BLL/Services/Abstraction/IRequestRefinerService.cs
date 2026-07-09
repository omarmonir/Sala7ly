using Sala7ly.BLL.DTOs.AiDTOs;

namespace Sala7ly.BLL.Services.Abstraction
{
    public interface IRequestRefinerService
    {
        Task<FollowUpDto> AskFollowUpAsync(FollowUpRequestDto dto, string? userId = null);
        Task<RefineResultDto> RefineAsync(RefineRequestDto dto, List<string> allAnswers, string? userId = null);
    }
}