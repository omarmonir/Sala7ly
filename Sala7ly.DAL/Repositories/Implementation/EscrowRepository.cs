using Microsoft.EntityFrameworkCore;
using Sala7ly.DAL.DataBase;
using Sala7ly.DAL.Entities;
using Sala7ly.DAL.Repositories.Abstraction;

namespace Sala7ly.DAL.Repositories.Implementation
{
    public class EscrowRepository : GenericRepository<EscrowTransaction>, IEscrowRepository
    {
        public EscrowRepository(AppDbContext context) : base(context) { }

        public async Task<EscrowTransaction?> GetByRequestIdAsync(int requestId)
        {
            return await _context.EscrowTransactions
                .Include(e => e.ServiceRequest)
                .Include(e => e.Customer).ThenInclude(c => c.User)
                .Include(e => e.Technician).ThenInclude(t => t.User)
                .FirstOrDefaultAsync(e => e.ServiceRequestId == requestId);
        }

        public async Task<EscrowTransaction?> GetByProviderRefAsync(string providerRef)
        {
            return await _context.EscrowTransactions
                .Include(e => e.ServiceRequest)
                .Include(e => e.Customer).ThenInclude(c => c.User)
                .Include(e => e.Technician).ThenInclude(t => t.User)
                .FirstOrDefaultAsync(e => e.ProviderRef == providerRef);
        }

        public async Task<EscrowTransaction?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.EscrowTransactions
                .Include(e => e.ServiceRequest)
                .Include(e => e.Customer).ThenInclude(c => c.User)
                .Include(e => e.Technician).ThenInclude(t => t.User)
                .FirstOrDefaultAsync(e => e.Id == id);
        }
    }
}
