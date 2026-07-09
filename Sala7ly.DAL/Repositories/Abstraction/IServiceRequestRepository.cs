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
        Task<ServiceRequest?> GetByIdWithDetailsAsync(int requestId);
        Task<IEnumerable<ServiceRequest>> GetAssignedByTechnicianUserIdAsync(string userId);

        /// <summary>
        /// Returns completed requests in the same category, including their
        /// AI summary and accepted bid price — used as RAG context for
        /// follow-up questions and price estimation.
        /// </summary>
        Task<IEnumerable<ServiceRequest>> GetCompletedByCategoryAsync(int categoryId, int limit = 20);
    }
}