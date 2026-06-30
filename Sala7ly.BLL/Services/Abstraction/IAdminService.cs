using Sala7ly.BLL.DTOs.AdminDTOs;

namespace Sala7ly.BLL.Services.Abstraction
{
    public interface IAdminService
    {
        Task<AdminDashboardDto> GetDashboardStatsAsync();
        Task<AdminWalletStatsDto> GetWalletStatsAsync();
    }
}