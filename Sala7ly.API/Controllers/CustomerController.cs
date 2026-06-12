using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sala7ly.BLL.DTOs.CustomerDTOs;
using Sala7ly.BLL.Services.Abstraction;
using System.Security.Claims;

namespace Sala7ly.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class CustomerController : ControllerBase
    {
      
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }


        // ── GET api/customer
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _customerService.GetAllAsync();
            return Ok(result);
        }



        // ── GET api/customer/{id} 
        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _customerService.GetByIdAsync(id);
            if (result is null) return NotFound();
            return Ok(result);
        }

        // ── POST api/customer 

        [HttpPost("register")]
        [AllowAnonymous]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Register([FromForm] CustomerRegisterDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var success = await _customerService.AddAsync(dto);
            if (!success) return Conflict(new { message = "البريد الإلكتروني مستخدم بالفعل" });

            return StatusCode(201, new { message = "تم إنشاء الحساب بنجاح" });
        }



        // ── PUT api/customer/{id} 
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Customer,Admin")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Update(int id, [FromForm] CustomerProfileUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var success = await _customerService.UpdateAsync(id, dto);
            if (!success) return NotFound();

            return Ok(new { message = "تم تحديث البيانات بنجاح" });
        }

        // ── DELETE api/customer/{id} 
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var deletedBy = User.Identity?.Name ?? "system";
            var success = await _customerService.DeleteAsync(id, deletedBy);
            if (!success) return NotFound();

            return Ok(new { message = "تم حذف الحساب بنجاح" });
        }



        // ── GET api/customer/me ───────────────────────────────
        [HttpGet("me")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMine()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _customerService.GetMineAsync(userId);

            if (result is null)
                return NotFound();

            return Ok(result);
        }



    }

}
