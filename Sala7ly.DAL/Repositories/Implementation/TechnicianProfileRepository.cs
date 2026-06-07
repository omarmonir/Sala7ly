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
            return await _context.TechnicianProfiles.ToListAsync();
        }

        public async Task<TechnicianProfile> GetByIdAsync(int id)
        {
            return await _context.TechnicianProfiles.FindAsync(id);
        }

        public async Task<TechnicianProfile> GetByUserIdAsync(string userId)
        {
            return await _context.TechnicianProfiles
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

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}