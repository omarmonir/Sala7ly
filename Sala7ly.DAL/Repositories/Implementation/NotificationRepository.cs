using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sala7ly.DAL.DataBase;
using Sala7ly.DAL.Entities;
using Sala7ly.DAL.Repositories.Abstraction;

namespace Sala7ly.DAL.Repositories.Implementation
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _context;

        public NotificationRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<Notification?> GetByIdAsync(int id)
            => _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.IsDeleted != true);

        public Task<List<Notification>> GetByUserIdAsync(string userId)
            => _context.Notifications
                .Where(n => n.UserId == userId && n.IsDeleted != true)
                .OrderByDescending(n => n.SentAt)
                .ToListAsync();

        public Task<List<Notification>> GetUnreadByUserIdAsync(string userId)
            => _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead && n.IsDeleted != true)
                .OrderByDescending(n => n.SentAt)
                .ToListAsync();

        public Task<int> GetUnreadCountAsync(string userId)
            => _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead && n.IsDeleted != true);

        public async Task AddAsync(Notification notification)
        {
            await _context.Notifications.AddAsync(notification);
        }

        public void Update(Notification notification)
        {
            _context.Notifications.Update(notification);
        }

        public Task<int> SaveChangesAsync()
            => _context.SaveChangesAsync();
    }
}