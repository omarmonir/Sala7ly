using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sala7ly.BLL.DTOs.BidDTOs;
using Sala7ly.BLL.DTOs.Common;
using Sala7ly.BLL.Services.Abstraction;

namespace Sala7ly.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api")]
    public class BidController : ControllerBase
    {
        private readonly IBidService _bidService;

        public BidController(IBidService bidService)
        {
            _bidService = bidService;
        }

        private string CurrentUserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // ── POST /api/requests/{requestId}/bids ───────────────
        [Authorize(Roles = "Technician")]
        [HttpPost("requests/{requestId}/bids")]
        public async Task<IActionResult> SubmitBid(int requestId, [FromBody] SubmitBidDto dto)
        {
            try
            {
                var result = await _bidService.SubmitBidAsync(requestId, dto, CurrentUserId);

                return Ok(new ApiResponse<BidDto>
                {
                    Success = true,
                    Message = "تم إرسال العرض بنجاح",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<BidDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        // ── GET /api/requests/{requestId}/bids ───────────────
        [HttpGet("requests/{requestId}/bids")]
        public async Task<IActionResult> GetBids(int requestId)
        {
            try
            {
                var result = await _bidService.GetBidsByRequestAsync(requestId);

                return Ok(new ApiResponse<List<BidDto>>
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<List<BidDto>>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        // ── GET /api/bids/{bidId} ─────────────────────────────
        [HttpGet("bids/{bidId}")]
        public async Task<IActionResult> GetBid(int bidId)
        {
            try
            {
                var result = await _bidService.GetBidByIdAsync(bidId);

                if (result == null)
                    return NotFound(new ApiResponse<BidDto>
                    {
                        Success = false,
                        Message = "العرض غير موجود"
                    });

                return Ok(new ApiResponse<BidDto>
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<BidDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        // ── PUT /api/bids/{bidId}/accept ──────────────────────
        [Authorize(Roles = "Customer")]
        [HttpPut("bids/{bidId}/accept")]
        public async Task<IActionResult> AcceptBid(int bidId)
        {
            try
            {
                await _bidService.AcceptBidAsync(bidId, CurrentUserId);

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "تم قبول العرض بنجاح"
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new ApiResponse<string>
                {
                    Success = false,
                    Message = "غير مصرح لك بهذا الإجراء"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        // ── PUT /api/bids/{bidId}/reject ──────────────────────
        [Authorize(Roles = "Customer")]
        [HttpPut("bids/{bidId}/reject")]
        public async Task<IActionResult> RejectBid(int bidId)
        {
            try
            {
                await _bidService.RejectBidAsync(bidId, CurrentUserId);

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "تم رفض العرض"
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new ApiResponse<string>
                {
                    Success = false,
                    Message = "غير مصرح لك بهذا الإجراء"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        // ── DELETE /api/bids/{bidId} ──────────────────────────
        [Authorize(Roles = "Technician")]
        [HttpDelete("bids/{bidId}")]
        public async Task<IActionResult> WithdrawBid(int bidId)
        {
            try
            {
                await _bidService.WithdrawBidAsync(bidId, CurrentUserId);

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "تم سحب العرض بنجاح"
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new ApiResponse<string>
                {
                    Success = false,
                    Message = "غير مصرح لك بهذا الإجراء"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
    }
}