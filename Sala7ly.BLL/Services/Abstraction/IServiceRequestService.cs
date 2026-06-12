using Sala7ly.BLL.DTOs.ServiceRequestDTOs;

namespace Sala7ly.BLL.Services.Abstraction
{
    public interface IServiceRequestService
    {
        Task<ServiceRequestDetailsDto?> GetByIdAsync(int id);
        Task<IEnumerable<ServiceRequestListItemDto>> GetByCustomerIdAsync(int customerId);
        Task<IEnumerable<ServiceRequestListItemDto>> GetOpenRequestsAsync();
        Task<IEnumerable<ServiceRequestListItemDto>> GetMineAsync(string userId);
        Task<bool> CreateAsync(string customerId, CreateServiceRequestDto dto);
        Task<bool> CompleteAsync(int id);
    }
}