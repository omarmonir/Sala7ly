using Sala7ly.BLL.DTOs.AiDTOs;

namespace Sala7ly.BLL.Services.Abstraction
{
    // Sala7ly.BLL/AI/Interfaces/IPriceEstimationService.cs
    public interface IPriceEstimationService
    {
        Task<PriceEstimateDto> EstimateAsync(int requestId);
    }
}