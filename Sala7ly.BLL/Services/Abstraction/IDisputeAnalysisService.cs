using Sala7ly.BLL.DTOs.AiDTOs;

namespace Sala7ly.BLL.Services.Abstraction
{
    // Sala7ly.BLL/AI/Interfaces/IDisputeAnalysisService.cs
    public interface IDisputeAnalysisService
    {
        Task<DisputeAnalysisDto> AnalyzeAsync(int disputeId);
    }
}