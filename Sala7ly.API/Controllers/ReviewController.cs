using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sala7ly.BLL.DTOs.ReviewDTOs;
using Sala7ly.BLL.Services.Abstraction;
using System.Security.Claims;

namespace Sala7ly.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _service;

        public ReviewController(IReviewService service)
        {
            _service = service;
        }

        // GET api/review/5
        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var review = await _service.GetByIdAsync(id);
            if (review is null) return NotFound();
            return Ok(review);
        }
        // PUT api/review/moderate  → admin edits/moderates a review
        [HttpPut("moderate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Moderate([FromBody] ModerateReviewDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(adminId)) return Unauthorized();

            var success = await _service.ModerateAsync(adminId, dto);
            if (!success) return NotFound();

            return Ok(new { message = "تم تعديل التقييم" });
        }

        // DELETE api/review/5  → admin deletes a review
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success) return NotFound();

            return Ok(new { message = "تم حذف التقييم" });
        }
        // GET api/review/request/5  → the review for a specific request
        [HttpGet("request/{requestId:int}")]
        [Authorize]
        public async Task<IActionResult> GetByRequest(int requestId)
        {
            var review = await _service.GetByRequestIdAsync(requestId);
            if (review is null) return NotFound();
            return Ok(review);
        }

        // GET api/review/technician/{technicianUserId}  → all reviews about a technician
        [HttpGet("technician/{technicianUserId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetForTechnician(string technicianUserId)
        {
            var reviews = await _service.GetForTechnicianAsync(technicianUserId);
            return Ok(reviews);
        }

        // POST api/review  → leave a review (reviewer = logged-in user)
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateReviewDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var reviewerUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(reviewerUserId)) return Unauthorized();

            var success = await _service.CreateAsync(reviewerUserId, dto);
            if (!success)
                return BadRequest(new { message = "تعذّر إضافة التقييم. تأكد من أن الطلب مكتمل وأنك طرف فيه ولم تقم بتقييمه مسبقاً." });

            return StatusCode(201, new { message = "تم إضافة التقييم بنجاح" });
        }

        // PUT api/review/reply  → technician replies to a review about them
        [HttpPut("reply")]
        [Authorize(Roles = "Technician")]
        public async Task<IActionResult> Reply([FromBody] TechnicianReplyDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var technicianUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(technicianUserId)) return Unauthorized();

            var success = await _service.AddTechnicianReplyAsync(technicianUserId, dto);
            if (!success)
                return BadRequest(new { message = "تعذّر إضافة الرد. تأكد من أن هذا التقييم يخصّك." });

            return Ok(new { message = "تم إضافة الرد بنجاح" });
        }
        // GET api/review  → all reviews
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var reviews = await _service.GetAllAsync();
            return Ok(reviews);
        }
    }
}