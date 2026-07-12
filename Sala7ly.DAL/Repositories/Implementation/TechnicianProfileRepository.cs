using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sala7ly.DAL.DataBase;
using Sala7ly.DAL.Entities;
using Sala7ly.DAL.Repositories.Abstraction;

namespace Sala7ly.DAL.Repositories.Implementation
{
    public class TechnicianProfileRepository : ITechnicianProfileRepository
    {
        private readonly AppDbContext _context;

        public TechnicianProfileRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<TechnicianProfile>> GetAllAsync()
        {
            return await _context.TechnicianProfiles
                .Include(u => u.User)
                
                .ToListAsync();
        }

        public async Task<TechnicianProfile> GetByIdAsync(int id)
        {
            return await _context.TechnicianProfiles
                .Include(u => u.User)
                .Include(tp => tp.Categories)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<TechnicianProfile> GetByUserIdAsync(string userId)
        {
            return await _context.TechnicianProfiles
                .Include(u => u.User)

                .FirstOrDefaultAsync(t => t.UserId == userId);
        }

        public Task<TechnicianProfile?> GetProfileByUserIdAsync(string userId)
            => _context.TechnicianProfiles
                .Include(tp => tp.User)
                .Include(tp => tp.Verifications)
                .Include(tp => tp.Portfolio)
                .Include(tp => tp.Categories)
                .FirstOrDefaultAsync(tp => tp.UserId == userId && tp.IsDeleted != true);

        // ── Category helpers ──────────────────────────────────────────────────
        public async Task<TechnicianProfile?> GetByIdWithCategoriesAsync(int id)
        {
            return await _context.TechnicianProfiles
                .Include(t => t.User)
                .Include(t => t.Categories)
                    .ThenInclude(c => c.Category)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<List<int>> GetCategoryIdsByUserIdAsync(string userId)
        {
            return await _context.TechnicianCategories
                .Where(tc => tc.Technician.UserId == userId
                          && tc.Technician.IsDeleted != true)
                .Select(tc => tc.CategoryId)
                .Distinct()
                .ToListAsync();
        }

         
        public async Task<List<string>> GetUserIdsByAnyCategoryAsync(IEnumerable<int> categoryIds)
        {
            var ids = categoryIds.ToList();

            return await _context.TechnicianCategories
                .Where(tc => ids.Contains(tc.CategoryId)
                          && tc.Technician.IsApproved
                          && tc.Technician.IsDeleted != true)
                .Select(tc => tc.Technician.UserId)
                .Distinct()
                .ToListAsync();
        }

        // ── CRUD ──────────────────────────────────────────────────────────────

        public async Task AddAsync(TechnicianProfile technician)
        {
            await _context.TechnicianProfiles.AddAsync(technician);
        }

        public void Update(TechnicianProfile technician)
        {
            _context.TechnicianProfiles.Update(technician);
        }

        public void Delete(TechnicianProfile technician)
        {
            _context.TechnicianProfiles.Remove(technician);
        }

        public Task<int> SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }
        public async Task<List<TechnicianProfile>> GetApprovedWithEmbeddingsAsync()
        {
            return await _context.TechnicianProfiles
                .Where(t => t.IsApproved
                         && t.EmbeddingVectorJson != null)
                .Include(t => t.User)
                .Include(t => t.Categories)
                    .ThenInclude(c => c.Category)
                .ToListAsync();
        }

        public async Task<List<TechnicianProfile>> GetWithOutdatedEmbeddingsAsync(int days)
        {
            var cutoff = DateTime.UtcNow.AddDays(-days);

            return await _context.TechnicianProfiles
                .Where(t => t.IsApproved
                         && (t.EmbeddingUpdatedAt == null
                          || t.EmbeddingUpdatedAt < cutoff))
                .ToListAsync();
        }
    }
}