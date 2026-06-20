using Sala7ly.DAL.Entities;

namespace Sala7ly.DAL.Repositories.Abstraction
{
    public interface IServiceRequestRepository : IGenericRepository<ServiceRequest>
    {
        Task<IEnumerable<ServiceRequest>> GetByCustomerIdAsync(int customerId);
        Task<IEnumerable<ServiceRequest>> GetOpenRequestsAsync();
        Task<ServiceRequest?> GetByIdWithPartiesAsync(int requestId);
        Task<IEnumerable<ServiceRequest>> GetAllAsync();
    }
}