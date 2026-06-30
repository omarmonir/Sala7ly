using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sala7ly.BLL.DTOs.ServiceRequestDTOs;
using Sala7ly.BLL.Services.Abstraction;
using System.Security.Claims;

namespace Sala7ly.API.Controllers
{
    [Route("api/requests")]
    [ApiController]
    public class ServiceRequestController : ControllerBase
    {
        private readonly IServiceRequestService _serviceRequestService;

        public ServiceRequestController(IServiceRequestService serviceRequestService)
        {
            _serviceRequestService = serviceRequestService;
        }

        // POST api/requests
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromForm] CreateServiceRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdBy = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(createdBy))
                return Unauthorized(new { Message = "Customer not found" });

            var result = await _serviceRequestService.CreateAsync(createdBy, dto);
            if (!result)
                return BadRequest(new { Message = "Failed to create request" });

            return StatusCode(201, new { Message = "Request created successfully" });
        }

        // GET api/requests  (Admin: all requests in any state)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var requests = await _serviceRequestService.GetAllAsync();
            return Ok(requests);
        }

        // GET api/requests/mine  (Customer: their own requests)
        [HttpGet("mine")]
        [Authorize]
        public async Task<IActionResult> GetMine()
        {
            var createdBy = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(createdBy))
                return Unauthorized(new { Message = "Customer not found" });

            var requests = await _serviceRequestService.GetMineAsync(createdBy);
            return Ok(requests);
        }

        // GET api/requests/open
        // - Technician  → only open requests whose category matches their specialisation
        // - Admin       → all open requests (unfiltered)
        // - Any other   → 403
        [HttpGet("open")]
        [Authorize(Roles = "Technician,Admin")]
        public async Task<IActionResult> GetOpen()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { Message = "User not found" });

            IEnumerable<Sala7ly.BLL.DTOs.ServiceRequestDTOs.ServiceRequestListItemDto> requests;

            if (User.IsInRole("Admin"))
            {
                // Admins see everything
                requests = await _serviceRequestService.GetOpenRequestsAsync();
            }
            else
            {
                // Technicians see only their category's requests
                requests = await _serviceRequestService.GetOpenRequestsForTechnicianAsync(userId);
            }

            return Ok(requests);
        }

        // GET api/requests/assigned  (Technician: tasks assigned to them)
        [HttpGet("assigned")]
        [Authorize(Roles = "Technician")]
        public async Task<IActionResult> GetAssigned()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { Message = "Technician not found" });

            var requests = await _serviceRequestService.GetAssignedAsync(userId);
            return Ok(requests);
        }

        // GET api/requests/{id}
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var request = await _serviceRequestService.GetByIdAsync(id);
            if (request is null)
                return NotFound(new { Message = "Request not found" });

            return Ok(request);
        }

        // PUT api/requests/{id}/complete  → customer marks completed
        [HttpPut("{id}/complete")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Complete(int id)
        {
            var result = await _serviceRequestService.CompleteAsync(id);
            if (!result)
                return NotFound(new { Message = "Request not found" });

            return Ok(new { Message = "Request completed successfully" });
        }

        // PUT api/requests/{id}  (Admin: update details)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateServiceRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _serviceRequestService.UpdateAsync(id, dto);
            if (!result)
                return NotFound(new { Message = "Request not found" });

            return Ok(new { Message = "Request updated successfully" });
        }

        // PUT api/requests/{id}/start  → technician marks work started
        [HttpPut("{id}/start")]
        [Authorize(Roles = "Technician")]
        public async Task<IActionResult> Start(int id)
        {
            var result = await _serviceRequestService.StartProgressAsync(id);
            if (!result)
                return NotFound(new { Message = "Request not found" });

            return Ok(new { Message = "Work started" });
        }

        // DELETE api/requests/{id}  (Admin only)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _serviceRequestService.DeleteAsync(id);
            if (!result)
                return NotFound(new { Message = "Request not found" });

            return Ok(new { Message = "Request deleted successfully" });
        }
    }
}