using Microsoft.EntityFrameworkCore;
using Sala7ly.DAL.DataBase;
using Sala7ly.DAL.Entities;
using Sala7ly.DAL.Enums;
using Sala7ly.DAL.Repositories.Abstraction;

namespace Sala7ly.DAL.Repositories.Implementation
{
    public class AdminRepository : IAdminRepository
    {
        private readonly AppDbContext _context;

        public AdminRepository(AppDbContext context)
        {
            _context = context;
        }


        public Task<int> GetTotalCustomersAsync()
            => _context.CustomerProfiles.CountAsync();

        public Task<int> GetTotalTechniciansAsync()
            => _context.TechnicianProfiles.CountAsync(t => t.IsApproved);

        public Task<int> GetActiveRequestsAsync()
            => _context.ServiceRequests.CountAsync(r =>
                r.Status == Status.open
                || r.Status == Status.assigned
                || r.Status == Status.in_progress);

        public Task<int> GetCompletedJobsAsync()
            => _context.ServiceRequests.CountAsync(r => r.Status == Status.completed);

        public async Task<decimal> GetMonthlyRevenueAsync(DateTime startOfMonth)
            => await _context.EscrowTransactions
                .Where(e => e.Status == EscrowStatus.Released
                         && e.ReleasedAt >= startOfMonth)
                .SumAsync(e => (decimal?)e.PlatformFee) ?? 0;

        public async Task<decimal> GetLastMonthRevenueAsync(DateTime lastMonth, DateTime startOfMonth)
            => await _context.EscrowTransactions
                .Where(e => e.Status == EscrowStatus.Released
                         && e.ReleasedAt >= lastMonth
                         && e.ReleasedAt < startOfMonth)
                .SumAsync(e => (decimal?)e.PlatformFee) ?? 0;


        public async Task<decimal> GetTotalDepositsAsync()
            => await _context.EscrowTransactions
                .Where(e => e.Status != EscrowStatus.PendingDeposit)
                .SumAsync(e => (decimal?)e.Amount) ?? 0;

        public async Task<decimal> GetTotalWithdrawalsAsync()
            => await _context.WalletTransactions
                .Where(t => t.Type == WalletTransactionType.withdrawal)
                .SumAsync(t => (decimal?)Math.Abs(t.Amount)) ?? 0;

        public async Task<decimal> GetPendingBalanceAsync()
            => await _context.EscrowTransactions
                .Where(e => e.Status == EscrowStatus.Held)
                .SumAsync(e => (decimal?)e.Amount) ?? 0;
    }
}
