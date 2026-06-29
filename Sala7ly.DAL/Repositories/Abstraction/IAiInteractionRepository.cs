using Sala7ly.DAL.Entities;

namespace Sala7ly.DAL.Repositories.Abstraction
{
    public interface IAiInteractionRepository
    {
        Task AddAsync(Ai_Interaction interaction);
        Task<List<Ai_Interaction>> GetByRequestIdAsync(int requestId);
        Task<List<Ai_Interaction>> GetByUserIdAsync(string userId);
        Task<int> SaveChangesAsync();
    }
}