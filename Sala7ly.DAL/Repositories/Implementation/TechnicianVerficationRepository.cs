using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sala7ly.DAL.DataBase;
using Sala7ly.DAL.Entities;
using Sala7ly.DAL.Repositories.Abstraction;

namespace Sala7ly.DAL.Repositories.Implementation
{
    public class TechnicianVerificationRepository : ITechnicianVerificationRepository
    {
        private readonly AppDbContext _context;

        public TechnicianVerificationRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<TechnicianVerification?> GetByIdAsync(int id)
    => _context.TechnicianVerifications
        .Include(v => v.Technician)
            .ThenInclude(t => t.User)
        .FirstOrDefaultAsync(v => v.Id == id && v.IsDeleted != true);

        public Task<List<TechnicianVerification>> GetByTechnicianIdAsync(int technicianId)
            => _context.TechnicianVerifications
                .Where(v => v.TechnicianId == technicianId && v.IsDeleted != true)
                .ToListAsync();

        public Task<List<TechnicianVerification>> GetPendingAsync()
            => _context.TechnicianVerifications
                .Include(v => v.Technician)
                .Where(v => v.Status == VerificationStatus.Pending && v.IsDeleted != true)
                .ToListAsync();

        public async Task AddAsync(TechnicianVerification verification)
        {
            await _context.TechnicianVerifications.AddAsync(verification);
        }

        public void Update(TechnicianVerification verification)
        {
            _context.TechnicianVerifications.Update(verification);
        }

        public Task<int> SaveChangesAsync()
            => _context.SaveChangesAsync();
    }
}