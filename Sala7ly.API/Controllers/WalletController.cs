using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sala7ly.BLL.DTOs.Common;
using Sala7ly.BLL.DTOs.WalletDTOs;
using Sala7ly.BLL.Services.Abstraction;

namespace Sala7ly.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/wallet")]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;

        public WalletController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        private string CurrentUserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpGet]
        public async Task<IActionResult> GetWallet(int page = 1, int pageSize = 20)
        {
            try
            {
                var result = await _walletService.GetWalletAsync(CurrentUserId, page, pageSize);
                return Ok(new ApiResponse<WalletDto> { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<WalletDto> { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("topup")]
        public async Task<IActionResult> TopUp([FromBody] TopUpDto dto)
        {
            try
            {
                var result = await _walletService.TopUpAsync(CurrentUserId, dto);
                return Ok(new ApiResponse<TopUpResultDto> { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<string> { Success = false, Message = ex.Message });
            }
        }

        [Authorize(Roles = "Technician")]
        [HttpPost("withdraw")]
        public async Task<IActionResult> Withdraw([FromBody] WithdrawDto dto)
        {
            try
            {
                await _walletService.WithdrawAsync(CurrentUserId, dto);
                return Ok(new ApiResponse<string> { Success = true, Message = "تم طلب السحب بنجاح" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<string> { Success = false, Message = ex.Message });
            }
        }
    }
}
