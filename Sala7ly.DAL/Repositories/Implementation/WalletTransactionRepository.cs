using Microsoft.EntityFrameworkCore;
using Sala7ly.DAL.DataBase;
using Sala7ly.DAL.Entities;
using Sala7ly.DAL.Enums;
using Sala7ly.DAL.Repositories.Abstraction;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sala7ly.DAL.Repositories.Implementation
{
    public class WalletTransactionRepository : GenericRepository<WalletTransaction>, IWalletTransactionRepository
    {
        public WalletTransactionRepository(AppDbContext context) : base(context) { }

        public async Task<List<WalletTransaction>> GetByWalletIdAsync(int walletId, int page, int pageSize)
        {
            return await _context.WalletTransactions
                .Where(t => t.WalletId == walletId)
                .OrderByDescending(t => t.CreatedOn)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<bool> ExistsByReferenceAsync(string reference, WalletTransactionType type, int walletId)
        {
            return await _context.WalletTransactions
                .AnyAsync(t => t.Reference == reference && t.Type == type && t.WalletId == walletId);
        }

        public async Task<WalletTransaction?> GetByReferenceAndTypeAsync(string reference, WalletTransactionType type)
        {
            return await _context.WalletTransactions
                .Where(t => t.Reference == reference && t.Type == type)
                .OrderByDescending(t => t.CreatedOn)
                .FirstOrDefaultAsync();
        }

        public async Task<List<WalletTransaction>> GetByReferenceAsync(string reference, WalletTransactionType type)
        {
            return await _context.WalletTransactions
                .Where(t => t.Reference == reference && t.Type == type)
                .OrderByDescending(t => t.CreatedOn)
                .ToListAsync();
        }
    }
}
