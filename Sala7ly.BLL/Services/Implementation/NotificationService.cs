using Microsoft.AspNetCore.SignalR;
using Sala7ly.API.Hubs;
using Sala7ly.BLL.DTOs.NotificationDTOs;
using Sala7ly.BLL.Services.Abstraction;
using Sala7ly.DAL.Entities;
using Sala7ly.DAL.Enums;
using Sala7ly.DAL.Repositories.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sala7ly.BLL.Services.Implementation
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repository;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(
            INotificationRepository repository,
            IHubContext<NotificationHub> hubContext)
        {
            _repository = repository;
            _hubContext = hubContext;
        }

        // ── Sending ──────────────────────────────────────────

        public async Task NotifyUserAsync(
            string userId,
            NotificationType type,
            string title,
            string body,
            string? actorId = null,
            string? deepLink = null,
            string? metadata = null)
        {
            // 1. build + save the notification
            var notification = new Notification
            {
                UserId = userId,
                ActorId = actorId,
                Type = type,
                Title = title,
                Body = body,
                DeepLink = deepLink,
                Metadata = metadata,
                IsRead = false,
                IsPushed = false,
                SentAt = DateTime.UtcNow
            };

            await _repository.AddAsync(notification);
            await _repository.SaveChangesAsync();

            // 2. push live to that specific user
            try
            {
                await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", new
                {
                    id = notification.Id,
                    type = notification.Type.ToString(),
                    title = notification.Title,
                    body = notification.Body,
                    deepLink = notification.DeepLink,
                    metadata = notification.Metadata,
                    sentAt = notification.SentAt
                });

                // 3. mark as pushed
                notification.IsPushed = true;
                _repository.Update(notification);
                await _repository.SaveChangesAsync();
            }
            catch
            {
                // user offline / no live connection — it's still saved,
                // so they'll see it on next load. IsPushed stays false.
            }
        }

        // ── Reading ──────────────────────────────────────────

        public async Task<List<NotificationResponseDto>> GetMyNotificationsAsync(string userId)
        {
            var items = await _repository.GetByUserIdAsync(userId);
            return items.Select(MapToDto).ToList();
        }

        public Task<int> GetUnreadCountAsync(string userId)
            => _repository.GetUnreadCountAsync(userId);

        public async Task<bool> MarkAsReadAsync(int id, string userId)
        {
            var notification = await _repository.GetByIdAsync(id);

            // not found, or belongs to another user → reject
            if (notification is null || notification.UserId != userId)
                return false;

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;

            _repository.Update(notification);
            await _repository.SaveChangesAsync();
            return true;
        }

        // ── Helper ───────────────────────────────────────────

        private static NotificationResponseDto MapToDto(Notification n)
        {
            return new NotificationResponseDto
            {
                Id = n.Id,
                Type = n.Type.ToString(),
                Title = n.Title,
                Body = n.Body,
                DeepLink = n.DeepLink,
                IsRead = n.IsRead,
                SentAt = n.SentAt
            };
        }
    }
}