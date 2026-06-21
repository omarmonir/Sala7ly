using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sala7ly.BLL.Services.Abstraction;
using System.Security.Claims;

namespace Sala7ly.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _service;

        public NotificationsController(INotificationService service)
        {
            _service = service;
        }

        // GET api/notifications  → my notifications
        [HttpGet]
        public async Task<IActionResult> GetMine()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var items = await _service.GetMyNotificationsAsync(userId);
            return Ok(items);
        }

        // GET api/notifications/unread-count  → badge number
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var count = await _service.GetUnreadCountAsync(userId);
            return Ok(new { count });
        }

        // PUT api/notifications/5/read  → mark one read
        [HttpPut("{id:int}/read")]
        public async Task<IActionResult> MarkRead(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var success = await _service.MarkAsReadAsync(id, userId);
            if (!success) return NotFound();
            return Ok(new { message = "تم وضع علامة مقروء" });
        }
    }
}