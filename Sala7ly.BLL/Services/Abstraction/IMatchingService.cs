using Sala7ly.BLL.DTOs.AiDTOs;
using Sala7ly.BLL.DTOs.ServiceRequestDTOs;

namespace Sala7ly.BLL.Services.Abstraction
{
    public interface IMatchingService
    {
        Task<List<TechnicianMatchDto>> FindMatchesAsync(int requestId, int topN = 10);
    }
}