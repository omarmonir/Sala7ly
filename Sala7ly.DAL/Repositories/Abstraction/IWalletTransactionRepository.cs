using Sala7ly.DAL.Entities;

namespace Sala7ly.DAL.Repositories.Abstraction
{
    public interface IWalletTransactionRepository : IGenericRepository<WalletTransaction>
    {
        Task<List<WalletTransaction>> GetByWalletIdAsync(int walletId, int page, int pageSize);
    }
}
