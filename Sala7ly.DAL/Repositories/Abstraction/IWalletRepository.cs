using Sala7ly.DAL.Entities;

namespace Sala7ly.DAL.Repositories.Abstraction
{
    public interface IWalletRepository : IGenericRepository<Wallet>
    {
        Task<Wallet?> GetByUserIdAsync(string userId);
        Task<Wallet?> GetByUserIdWithTransactionsAsync(string userId, int page, int pageSize);
        Task<bool> ExistsByUserIdAsync(string userId);
    }
}
