using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sala7ly.BLL.DTOs.ChatDTOs;
using Sala7ly.BLL.Services.Abstraction;


namespace Sala7ly.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        public ChatController(IChatService chatService) => _chatService = chatService;

        private string UserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpGet("{requestId:int}")]
        public async Task<IActionResult> GetHistory(
            int requestId, [FromQuery] int page = 1, [FromQuery] int pageSize = 30)
        {
            try
            {
                var messages = await _chatService.GetHistoryAsync(UserId, requestId, page, pageSize);
                return Ok(messages);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SendFile(
            [FromForm] SendMessageDto dto,
            [FromForm] IFormFileCollection files)
        {
            try
            {
                var saved = await _chatService.SaveMessageAsync(UserId, dto, files);
                return Ok(saved);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("{requestId:int}/read")]
        public async Task<IActionResult> MarkRead(int requestId)
        {
            try
            {
                await _chatService.MarkAsReadAsync(requestId, UserId);
                return Ok(new { message = "تم تحديد الرسائل كمقروءة." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations()
        {
            try
            {
                var list = await _chatService.GetConversationsAsync(UserId);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
