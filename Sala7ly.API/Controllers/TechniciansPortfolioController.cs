using Microsoft.AspNetCore.Mvc;
using Sala7ly.BLL.Dtos.TechnicianPortfolio;
using Sala7ly.BLL.Services.Abstraction;

namespace Sala7ly.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TechnicianPortfoliosController : ControllerBase
    {
        private readonly ITechnicianPortfolioService _service;

        public TechnicianPortfoliosController(ITechnicianPortfolioService service)
        {
            _service = service;
        }

        // GET: api/technicianportfolios
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _service.GetAllAsync();
            return Ok(items);
        }

        // GET: api/technicianportfolios/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null)
                return NotFound();

            return Ok(item);
        }

        // GET: api/technicianportfolios/technician/5
        [HttpGet("technician/{technicianId}")]
        public async Task<IActionResult> GetByTechnicianId(int technicianId)
        {
            var items = await _service.GetByTechnicianIdAsync(technicianId);
            return Ok(items);
        }

        // POST: api/technicianportfolios
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTechnicianPortfolioDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // PUT: api/technicianportfolios
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateTechnicianPortfolioDto dto)
        {
            var updated = await _service.UpdateAsync(dto);
            if (updated == null)
                return NotFound();

            return Ok(updated);
        }

        // DELETE: api/technicianportfolios/5
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