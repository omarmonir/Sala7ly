using Microsoft.AspNetCore.Mvc;
using Sala7ly.BLL.DTOs.TechnicianDTOs;
using Sala7ly.BLL.Services.Abstraction;

namespace Sala7ly.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TechniciansController : ControllerBase
    {
        private readonly ITechnicianService _service;

        public TechniciansController(ITechnicianService service)
        {
            _service = service;
        }

        // GET: api/technicians
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var technicians = await _service.GetAllAsync();
            return Ok(technicians);
        }

        // GET: api/technicians/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var technician = await _service.GetByIdAsync(id);
            if (technician is null)
                return NotFound();

            return Ok(technician);
        }

        // POST: api/technicians
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] TechnicianRegisterDto dto)
        {
            var created = await _service.AddAsync(dto);
            if (!created)
                return BadRequest("Could not create technician. The email may already be in use.");

            return Ok();
        }

        // PUT: api/technicians/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TechnicianProfileUpdateDto dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
            if (!updated)
                return NotFound();

            return NoContent();
        }

        // DELETE: api/technicians/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // deletedBy — ideally the current admin's id; placeholder for now
            var deletedBy = User?.Identity?.Name ?? "system";

            var deleted = await _service.DeleteAsync(id, deletedBy);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}