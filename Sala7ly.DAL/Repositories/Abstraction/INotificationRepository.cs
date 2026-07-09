using System.Collections.Generic;
using System.Threading.Tasks;
using Sala7ly.DAL.Entities;

namespace Sala7ly.DAL.Repositories.Abstraction
{
    public interface INotificationRepository
    {
        Task<Notification?> GetByIdAsync(int id);
        Task<List<Notification>> GetByUserIdAsync(string userId);
        Task<List<Notification>> GetUnreadByUserIdAsync(string userId);
        Task<int> GetUnreadCountAsync(string userId);
        Task AddAsync(Notification notification);
        void Update(Notification notification);
        Task<int> SaveChangesAsync();
    }
}