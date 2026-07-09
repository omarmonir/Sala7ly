using Sala7ly.BLL.DTOs.AiDTOs;

namespace Sala7ly.BLL.Services.Abstraction
{
    /// <summary>
    /// Semantic + LLM-reranked search for technicians who best fit an
    /// existing service request. Read-only — does not mutate the request or
    /// send notifications (see IRequestDispatchService for that).
    /// </summary>
    public interface ITechnicianMatchingService
    {
        Task<List<TechnicianMatchDto>> FindMatchesAsync(int requestId, int topN = 10);
    }
}