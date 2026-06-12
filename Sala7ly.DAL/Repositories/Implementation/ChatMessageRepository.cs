using Microsoft.EntityFrameworkCore;
using Sala7ly.DAL.DataBase;
using Sala7ly.DAL.Entities;
using Sala7ly.DAL.Repositories.Abstraction;

namespace Sala7ly.DAL.Repositories.Implementation
{
    public class ChatMessageRepository : GenericRepository<ChatMessage>, IChatMessageRepository
    {
        public ChatMessageRepository(AppDbContext db) : base(db) { }

        public async Task<List<ChatMessage>> GetByRequestIdAsync(
            int requestId, int page, int pageSize)
            => await _dbSet
                .Include(m => m.Sender)
                .Where(m => m.RequestId == requestId)
                .OrderByDescending(m => m.SentAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

        public async Task<List<ChatMessage>> GetUnreadAsync(int requestId, string userId)
            => await _dbSet
                .Where(m => m.RequestId == requestId &&
                            m.SenderId != userId &&
                            !m.IsRead)
                .ToListAsync();

        public async Task MarkAllAsReadAsync(int requestId, string userId)
        {
            var unread = await GetUnreadAsync(requestId, userId);
            foreach (var m in unread)
                m.MarkAsRead();
        }
        public async Task<ChatMessage?> GetByIdWithSenderAsync(int id)
    => await _dbSet
        .Include(m => m.Sender)
        .FirstOrDefaultAsync(m => m.Id == id);

        public async Task<List<int>> GetRequestIdsForUserAsync(string userId)
        {
            var asCustomer = await _context.CustomerProfiles
                .Where(c => c.UserId == userId)
                .SelectMany(c => c.ServiceRequests)
                .Select(r => r.Id)
                .ToListAsync();

            var asTechnician = await _context.TechnicianProfiles
                .Where(t => t.UserId == userId)
                .SelectMany(t => t.Bids)
                .Where(b => b.ServiceRequest.SelectedBidId == b.Id)
                .Select(b => b.ServiceRequestId)
                .ToListAsync();

            return asCustomer.Union(asTechnician).Distinct().ToList();
        }

        public async Task<ChatMessage?> GetLastMessageAsync(int requestId)
            => await _dbSet
                .Where(m => m.RequestId == requestId)
                .OrderByDescending(m => m.SentAt)
                .Include(m => m.Sender)
                .FirstOrDefaultAsync();

        public async Task<int> GetUnreadCountAsync(int requestId, string userId)
            => await _dbSet.CountAsync(m =>
                m.RequestId == requestId &&
                m.SenderId != userId &&
                !m.IsRead);
    }
}
