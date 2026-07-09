using Sala7ly.BLL.DTOs.ServiceRequestDTOs;

namespace Sala7ly.BLL.Services.Abstraction
{
    public interface IServiceRequestService
    {
        Task<ServiceRequestDetailsDto?> GetByIdAsync(int id);
        Task<IEnumerable<ServiceRequestListItemDto>> GetByCustomerIdAsync(int customerId);

        
        Task<IEnumerable<ServiceRequestListItemDto>> GetOpenRequestsAsync();

        
        Task<IEnumerable<ServiceRequestListItemDto>> GetOpenRequestsForTechnicianAsync(string technicianUserId);

        Task<IEnumerable<ServiceRequestListItemDto>> GetMineAsync(string userId);
        Task<IEnumerable<ServiceRequestListItemDto>> GetAllAsync();
        Task<IEnumerable<ServiceRequestListItemDto>> GetAssignedAsync(string userId);
        Task<bool> CreateAsync(string customerId, CreateServiceRequestDto dto);
        Task<bool> StartProgressAsync(int id);
        Task<bool> CompleteAsync(int id);
        Task<bool> UpdateAsync(int id, UpdateServiceRequestDto dto);
        Task<bool> DeleteAsync(int id);
    }
}