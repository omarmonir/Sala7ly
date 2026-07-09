using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sala7ly.DAL.DataBase;
using Sala7ly.DAL.Entities;
using Sala7ly.DAL.Repositories.Abstraction;

namespace Sala7ly.DAL.Repositories.Implementation
{
    public class DisputeRepository : IDisputeRepository
    {
        private readonly AppDbContext _context;

        public DisputeRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<Dispute?> GetByIdAsync(int id)
            => _context.Disputes
                .Include(d => d.ServiceRequest)
                .Include(d => d.EscrowTransaction)
                .FirstOrDefaultAsync(d => d.Id == id);

        public Task<List<Dispute>> GetAllAsync()
            => _context.Disputes
                .Include(d => d.ServiceRequest)
                .Include(d => d.EscrowTransaction)
                .ToListAsync();

        public async Task AddAsync(Dispute dispute)
        {
            await _context.Disputes.AddAsync(dispute);
        }

        public void Update(Dispute dispute)
        {
            _context.Disputes.Update(dispute);
        }

        public Task<int> SaveChangesAsync()
            => _context.SaveChangesAsync();
    }
}
