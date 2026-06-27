using Sala7ly.DAL.Entities;

namespace Sala7ly.DAL.Repositories.Abstraction
{
    public interface IServiceRequestRepository : IGenericRepository<ServiceRequest>
    {
        Task<IEnumerable<ServiceRequest>> GetByCustomerIdAsync(int customerId);
        Task<IEnumerable<ServiceRequest>> GetOpenRequestsAsync();
        Task<IEnumerable<ServiceRequest>> GetOpenRequestsByCategoryIdsAsync(IEnumerable<int> categoryIds);
        Task<ServiceRequest?> GetByIdWithPartiesAsync(int requestId);
        Task<IEnumerable<ServiceRequest>> GetAllAsync();
        Task<IEnumerable<ServiceRequest>> GetAssignedByTechnicianUserIdAsync(string userId);
    }
}