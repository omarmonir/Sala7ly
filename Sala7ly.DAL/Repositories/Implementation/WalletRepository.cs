using Microsoft.EntityFrameworkCore;
using Sala7ly.DAL.DataBase;
using Sala7ly.DAL.Entities;
using Sala7ly.DAL.Repositories.Abstraction;

namespace Sala7ly.DAL.Repositories.Implementation
{
    public class WalletRepository : GenericRepository<Wallet>, IWalletRepository
    {
        public WalletRepository(AppDbContext context) : base(context) { }

        public async Task<Wallet?> GetByUserIdAsync(string userId)
        {
            return await _context.Wallets
                .FirstOrDefaultAsync(w => w.UserId == userId);
        }

        public async Task<Wallet?> GetByUserIdWithTransactionsAsync(string userId, int page, int pageSize)
        {
            return await _context.Wallets
                .Include(w => w.Transactions
                    .OrderByDescending(t => t.CreatedOn)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize))
                .FirstOrDefaultAsync(w => w.UserId == userId);
        }

        public async Task<bool> ExistsByUserIdAsync(string userId)
        {
            return await _context.Wallets.AnyAsync(w => w.UserId == userId);
        }
    }
}
