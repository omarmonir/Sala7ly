using Sala7ly.BLL.DTOs.AiDTOs;
using Sala7ly.DAL.Entities;

namespace Sala7ly.BLL.Services.Abstraction
{
     
    public interface ISmartMatchingService
    {
         
        Task<SmartMatchingResultDto> MatchAndNotifyAsync(
            ServiceRequest request,
            string customerUserId);
    }
}