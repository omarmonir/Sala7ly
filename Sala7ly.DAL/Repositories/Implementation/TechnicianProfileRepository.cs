using System.Collections.Generic;
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
            return await _context.TechnicianProfiles.Include(u =>u.User).ToListAsync();
        }

        public async Task<TechnicianProfile> GetByIdAsync(int id)
        {
            return await _context.TechnicianProfiles.Include(u => u.User).FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<TechnicianProfile> GetByUserIdAsync(string userId)
        {
            return await _context.TechnicianProfiles.Include(u => u.User)
                .FirstOrDefaultAsync(t => t.UserId == userId);
        }

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
        public Task<TechnicianProfile?> GetProfileByUserIdAsync(string userId)
            => _context.TechnicianProfiles
                .Include(tp => tp.User)
                .Include(tp => tp.Verifications)
                .Include(tp => tp.Portfolio)
                .Include(tp => tp.Categories)
                .FirstOrDefaultAsync(tp => tp.UserId == userId && tp.IsDeleted != true);
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}