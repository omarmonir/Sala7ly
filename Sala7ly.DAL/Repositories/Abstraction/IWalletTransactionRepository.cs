using Sala7ly.DAL.Entities;
using Sala7ly.DAL.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sala7ly.DAL.Repositories.Abstraction
{
    public interface IWalletTransactionRepository : IGenericRepository<WalletTransaction>
    {
        Task<List<WalletTransaction>> GetByWalletIdAsync(int walletId, int page, int pageSize);
        Task<bool> ExistsByReferenceAsync(string reference, WalletTransactionType type, int walletId);
        Task<WalletTransaction?> GetByReferenceAndTypeAsync(string reference, WalletTransactionType type);
        Task<List<WalletTransaction>> GetByReferenceAsync(string reference, WalletTransactionType type);
    }
}
