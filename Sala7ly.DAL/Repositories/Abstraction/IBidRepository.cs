using Sala7ly.DAL.Entities;

namespace Sala7ly.DAL.Repositories.Abstraction
{
    public interface IBidRepository : IGenericRepository<Bid>
    {
        Task<Bid?> GetByIdWithDetailsAsync(int bidId);
        Task<Bid?> GetByRequestAndTechnicianAsync(int requestId, int technicianId);
        Task<List<Bid>> GetByRequestIdAsync(int requestId);
        Task<List<Bid>> GetPendingByRequestAsync(int requestId, int excludeBidId);
        Task<List<Bid>> GetByTechnicianIdAsync(int technicianId);
        Task<List<Bid>> GetAllWithDetailsAsync();
        Task<int> CountByRequestAsync(int requestId);
        Task<List<Bid>> GetExpiredBidsAsync();
        Task<List<decimal>> GetAcceptedPricesByCategoryAsync(int categoryId, int limit);
        Task<bool> HasTechnicianBidAsync(int requestId, int technicianId);
    }
}
