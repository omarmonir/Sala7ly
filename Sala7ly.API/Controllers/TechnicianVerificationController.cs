using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sala7ly.BLL.DTOs.VerificationDTOs;
using Sala7ly.BLL.Services.Abstraction;
using System.Security.Claims;

namespace Sala7ly.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TechnicianVerificationController : ControllerBase
    {
        private readonly ITechnicianVerificationService _service;

        public TechnicianVerificationController(ITechnicianVerificationService service)
        {
            _service = service;
        }

        // ── GET api/technicianverification/{id}
        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result is null) return NotFound();
            return Ok(result);
        }

        // ── GET api/technicianverification/technician/{technicianId}
        [HttpGet("technician/{technicianId:int}")]
        [Authorize]
        public async Task<IActionResult> GetByTechnician(int technicianId)
        {
            var result = await _service.GetByTechnicianIdAsync(technicianId);
            return Ok(result);
        }

        // ── GET api/technicianverification/pending  (admin review queue)
        [HttpGet("pending")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPending()
        {
            var result = await _service.GetPendingAsync();
            return Ok(result);
        }

        // ── POST api/technicianverification/submit  (technician uploads a document)
        [HttpPost("submit")]
        [Authorize(Roles = "Technician")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Submit([FromForm] SubmitVerificationDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var success = await _service.SubmitAsync(dto);
            if (!success)
                return BadRequest(new { message = "تعذّر رفع المستند. تأكد من الملف ونوع المستند." });

            return StatusCode(201, new { message = "تم رفع المستند بنجاح" });
        }

        // ── PUT api/technicianverification/{id}/approve  (admin approves)
        [HttpPut("{id:int}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(int id)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(adminId)) return Unauthorized();

            var success = await _service.ApproveAsync(id, adminId);
            if (!success) return NotFound();

            return Ok(new { message = "تم اعتماد المستند" });
        }

        // ── PUT api/technicianverification/reject  (admin rejects with a reason)
        [HttpPut("reject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reject([FromBody] RejectVerificationDto dto)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(adminId)) return Unauthorized();

            var success = await _service.RejectAsync(dto, adminId);
            if (!success) return NotFound();

            return Ok(new { message = "تم رفض المستند" });
        }
    }
}