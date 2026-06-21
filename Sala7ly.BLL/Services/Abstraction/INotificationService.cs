using Sala7ly.BLL.DTOs.NotificationDTOs;
using Sala7ly.DAL.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sala7ly.BLL.Services.Abstraction
{
    public interface INotificationService
    {
        // ── sending (called internally from other services) ──
        Task NotifyUserAsync(
            string userId,
            NotificationType type,
            string title,
            string body,
            string? actorId = null,
            string? deepLink = null,
            string? metadata = null);

        // ── reading (exposed via the controller) ──
        Task<List<NotificationResponseDto>> GetMyNotificationsAsync(string userId);
        Task<int> GetUnreadCountAsync(string userId);
        Task<bool> MarkAsReadAsync(int id, string userId);
    }
}