using Microsoft.AspNetCore.Mvc;
using Sala7ly.BLL.Dtos.TechnicianProfile;
using Sala7ly.BLL.Services.Abstraction;

namespace Sala7ly.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TechnicianProfilesController : ControllerBase
    {
        private readonly ITechnicianProfileService _service;

        public TechnicianProfilesController(ITechnicianProfileService service)
        {
            _service = service;
        }

        // GET: api/technicianprofiles
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var technicians = await _service.GetAllAsync();
            return Ok(technicians);
        }

        // GET: api/technicianprofiles/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var technician = await _service.GetByIdAsync(id);
            if (technician == null)
                return NotFound();

            return Ok(technician);
        }

        // POST: api/technicianprofiles
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTechnicianProfileDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // PUT: api/technicianprofiles
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateTechnicianProfileDto dto)
        {
            var updated = await _service.UpdateAsync(dto);
            if (updated == null)
                return NotFound();

            return Ok(updated);
        }

        // DELETE: api/technicianprofiles/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}