using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sala7ly.BLL.DTOs.AiSupportDTOs;
using Sala7ly.BLL.Services.Abstraction;
using System.Security.Claims;

namespace Sala7ly.API.Controllers
{
    [ApiController]
    [Route("api/ai-support")]
    public class AiSupportController : ControllerBase
    {
        private readonly IAiSupportService _service;

        public AiSupportController(IAiSupportService service)
        {
            _service = service;
        }

        [HttpPost("ask")]
        [Authorize]
        public async Task<IActionResult> Ask([FromBody] AiSupportRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Message))
                return BadRequest(new { message = "الرسالة فارغة." });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _service.AskAsync(userId, dto);
            return Ok(result);
        }
    }
}