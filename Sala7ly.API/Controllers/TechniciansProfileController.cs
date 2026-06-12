using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        // POST api/technician/register
        [HttpPost("register")]
        [AllowAnonymous]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Register([FromForm] TechnicianRegisterDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var success = await _technicianService.AddAsync(dto);
            if (!success) return Conflict(new { message = "البريد الإلكتروني مستخدم بالفعل" });

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
}