using Sala7ly.BLL.DTOs.AdminDTOs;
using Sala7ly.BLL.Services.Abstraction;
using Sala7ly.DAL.Repositories.Abstraction;

namespace Sala7ly.BLL.Services.Implementation
{

    public partial class MatchingService
    {
        public class AdminService : IAdminService
        {
            private readonly IAdminRepository _adminRepo;

            public AdminService(IAdminRepository adminRepo)
            {
                _adminRepo = adminRepo;
            }


            public async Task<AdminDashboardDto> GetDashboardStatsAsync()
            {
                var now = DateTime.UtcNow;
                var startOfMonth = new DateTime(now.Year, now.Month, 1);
                var lastMonth = startOfMonth.AddMonths(-1);

                var monthlyRevenue = await _adminRepo.GetMonthlyRevenueAsync(startOfMonth);
                var lastMonthRevenue = await _adminRepo.GetLastMonthRevenueAsync(lastMonth, startOfMonth);

                double growthPercent = 0;
                if (lastMonthRevenue > 0)
                    growthPercent = Math.Round(
                        (double)((monthlyRevenue - lastMonthRevenue) / lastMonthRevenue * 100), 1);

                return new AdminDashboardDto
                {
                    TotalCustomers = await _adminRepo.GetTotalCustomersAsync(),
                    TotalTechnicians = await _adminRepo.GetTotalTechniciansAsync(),
                    ActiveRequests = await _adminRepo.GetActiveRequestsAsync(),
                    CompletedJobs = await _adminRepo.GetCompletedJobsAsync(),
                    MonthlyRevenue = monthlyRevenue,
                    GrowthPercent = growthPercent
                };
            }


            public async Task<AdminWalletStatsDto> GetWalletStatsAsync()
            {
                return new AdminWalletStatsDto
                {
                    TotalDeposits = await _adminRepo.GetTotalDepositsAsync(),
                    TotalWithdrawals = await _adminRepo.GetTotalWithdrawalsAsync(),
                    PendingBalance = await _adminRepo.GetPendingBalanceAsync()
                };
            }
        }

    }
}