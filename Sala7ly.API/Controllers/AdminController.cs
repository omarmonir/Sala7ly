using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sala7ly.BLL.DTOs.AdminDTOs;
using Sala7ly.BLL.DTOs.Common;
using Sala7ly.BLL.Services.Abstraction;

namespace Sala7ly.API.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

 
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            try
            {
                var result = await _adminService.GetDashboardStatsAsync();

                return Ok(new ApiResponse<AdminDashboardDto>
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<AdminDashboardDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }


        [HttpGet("wallet-stats")]
        public async Task<IActionResult> GetWalletStats()
        {
            try
            {
                var result = await _adminService.GetWalletStatsAsync();

                return Ok(new ApiResponse<AdminWalletStatsDto>
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<AdminWalletStatsDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
    }
}
