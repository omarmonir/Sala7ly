using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sala7ly.BLL.DTOs.BidDTOs;
using Sala7ly.BLL.DTOs.TechnicianDTOs;
using Sala7ly.BLL.Services.Abstraction;

namespace Sala7ly.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TechnicianController : ControllerBase
    {
        private readonly ITechnicianService _technicianService;

        public TechnicianController(ITechnicianService technicianService)
        {
            _technicianService = technicianService;
        }

        // GET api/technician
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _technicianService.GetAllAsync();
            return Ok(result);
        }

        // GET api/technician/5
        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _technicianService.GetByIdAsync(id);
            if (result is null) return NotFound();
            return Ok(result);
        }

        // GET api/technician/mine
        [HttpGet("mine")]
        [Authorize]
        public async Task<IActionResult> GetMine()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { Message = "Technician not found" });

            var profile = await _technicianService.GetByUserIdAsync(userId);

            if (profile is null)
                return NotFound(new { Message = "Technician profile not found" });

            return Ok(profile);
        }

        // POST api/technician/register
        [HttpPost("register")]
        [AllowAnonymous]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Register([FromForm] TechnicianRegisterDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var success = await _technicianService.AddAsync(dto);
            if (!success)
                return BadRequest(new { message = "تعذّر إنشاء الحساب. تأكد من أن البريد غير مستخدم وأن كلمة المرور تستوفي الشروط (8 أحرف على الأقل، تحتوي على حرف كبير وصغير ورقم ورمز خاص)." });

            return StatusCode(201, new { message = "تم إنشاء حساب الفني بنجاح، في انتظار الموافقة" });
        }

        // PUT api/technician/5
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Technician,Admin")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Update(int id, [FromForm] TechnicianProfileUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var success = await _technicianService.UpdateAsync(id, dto);
            if (!success) return NotFound();

            return Ok(new { message = "تم تحديث بيانات الفني بنجاح" });
        }

        // DELETE api/technician/5
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var deletedBy = User.Identity?.Name ?? "system";
            var success = await _technicianService.DeleteAsync(id, deletedBy);
            if (!success) return NotFound();

            return Ok(new { message = "تم حذف حساب الفني بنجاح" });
        }
    }
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