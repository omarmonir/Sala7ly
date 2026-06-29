using Microsoft.EntityFrameworkCore;
using Sala7ly.DAL.DataBase;
using Sala7ly.DAL.Entities;
using Sala7ly.DAL.Repositories.Abstraction;

namespace Sala7ly.DAL.Repositories.Implementation
{
    public class AiInteractionRepository : IAiInteractionRepository
    {
        private readonly AppDbContext _context;

        public AiInteractionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Ai_Interaction interaction)
        {
            await _context.AiInteractions.AddAsync(interaction);
        }

        public async Task<List<Ai_Interaction>> GetByRequestIdAsync(int requestId)
        {
            return await _context.AiInteractions
                .Where(a => a.RequestId == requestId)
                .OrderByDescending(a => a.CreatedOn)
                .ToListAsync();
        }

        public async Task<List<Ai_Interaction>> GetByUserIdAsync(string userId)
        {
            return await _context.AiInteractions
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedOn)
                .ToListAsync();
        }

        public Task<int> SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }
    }
}
