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

        // GET api/requests/mine
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
        [HttpGet("open")]
        [Authorize]
        public async Task<IActionResult> GetOpen()
        {
            var requests = await _serviceRequestService.GetOpenRequestsAsync();
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

        // PUT api/requests/{id}/complete
        [HttpPut("{id}/complete")]
        [Authorize]
        public async Task<IActionResult> Complete(int id)
        {
            var result = await _serviceRequestService.CompleteAsync(id);
            if (!result)
                return NotFound(new { Message = "Request not found" });

            return Ok(new { Message = "Request completed successfully" });
        }
    }
}
