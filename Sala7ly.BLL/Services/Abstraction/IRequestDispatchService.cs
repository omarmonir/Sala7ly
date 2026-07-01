using Sala7ly.BLL.DTOs.AiDTOs;
using Sala7ly.DAL.Entities;

namespace Sala7ly.BLL.Services.Abstraction
{
    /// <summary>
    /// Runs once, right after a ServiceRequest is created: verifies/repairs
    /// the chosen category via the LLM, then notifies matching technicians
    /// (DB notification + SignalR broadcast).
    /// </summary>
    public interface IRequestDispatchService
    {
        Task<SmartMatchingResultDto> MatchAndNotifyAsync(
            ServiceRequest request,
            string customerUserId);
    }
}