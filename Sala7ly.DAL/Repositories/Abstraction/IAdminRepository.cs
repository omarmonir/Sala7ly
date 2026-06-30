namespace Sala7ly.DAL.Repositories.Abstraction
{
    public interface IAdminRepository
    {
        Task<int> GetTotalCustomersAsync();
        Task<int> GetTotalTechniciansAsync();
        Task<int> GetActiveRequestsAsync();
        Task<int> GetCompletedJobsAsync();
        Task<decimal> GetMonthlyRevenueAsync(DateTime startOfMonth);
        Task<decimal> GetLastMonthRevenueAsync(DateTime lastMonth, DateTime startOfMonth);
        Task<decimal> GetTotalDepositsAsync();
        Task<decimal> GetTotalWithdrawalsAsync();
        Task<decimal> GetPendingBalanceAsync();
    }
}
