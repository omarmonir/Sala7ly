using Sala7ly.DAL.Entities;

namespace Sala7ly.DAL.Repositories.Abstraction
{
    public interface IChatMessageRepository : IGenericRepository<ChatMessage>
    {
        Task<List<ChatMessage>> GetByRequestIdAsync(int requestId, int page, int pageSize);
        Task<List<ChatMessage>> GetUnreadAsync(int requestId, string userId);
        Task MarkAllAsReadAsync(int requestId, string userId);
        Task<ChatMessage?> GetByIdWithSenderAsync(int id);
        Task<List<int>> GetRequestIdsForUserAsync(string userId); // ← only IDs
        Task<ChatMessage?> GetLastMessageAsync(int requestId);
        Task<int> GetUnreadCountAsync(int requestId, string userId);
    }
}
