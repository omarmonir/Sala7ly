using Microsoft.EntityFrameworkCore;
using Sala7ly.DAL.DataBase;
using Sala7ly.DAL.Entities;
using Sala7ly.DAL.Enums;
using Sala7ly.DAL.Repositories.Abstraction;

namespace Sala7ly.DAL.Repositories.Implementation
{
    public class BidRepository : GenericRepository<Bid>, IBidRepository
    {
        public BidRepository(AppDbContext context) : base(context) { }

        public async Task<List<Bid>> GetAllWithDetailsAsync()
        {
            return await _context.Bids
                .Include(b => b.ServiceRequest)
                    .ThenInclude(r => r.Profile)
                        .ThenInclude(c => c.User)
                .Include(b => b.ServiceRequest)
                    .ThenInclude(r => r.Category)
                .Include(b => b.Technician)
                    .ThenInclude(t => t.User)
                .OrderByDescending(b => b.SubmittedAt)
                .ToListAsync();
        }
        public async Task<Bid?> GetByIdWithDetailsAsync(int bidId)
        {
            return await _context.Bids
                .Include(b => b.ServiceRequest)
                    .ThenInclude(r => r.Profile)
                        .ThenInclude(c => c.User)
                .Include(b => b.Technician)
                    .ThenInclude(t => t.User)
                .FirstOrDefaultAsync(b => b.Id == bidId);
        }

        public async Task<Bid?> GetByRequestAndTechnicianAsync(int requestId, int technicianId)
        {
            return await _context.Bids
                .FirstOrDefaultAsync(b => b.ServiceRequestId == requestId
                                       && b.TechnicianId == technicianId
                                       && b.Status != BidStatus.withdrawn);
        }

        public async Task<List<Bid>> GetByRequestIdAsync(int requestId)
        {
            return await _context.Bids
                .Where(b => b.ServiceRequestId == requestId
                         && b.Status != BidStatus.withdrawn
                         && b.Status != BidStatus.expired)
                .Include(b => b.Technician)
                    .ThenInclude(t => t.User)
                .OrderBy(b => b.Price)
                .ToListAsync();
        }

        public async Task<List<Bid>> GetPendingByRequestAsync(int requestId, int excludeBidId)
        {
            return await _context.Bids
                .Where(b => b.ServiceRequestId == requestId
                         && b.Status == BidStatus.pending
                         && b.Id != excludeBidId)
                .Include(b => b.Technician)
                    .ThenInclude(t => t.User)
                .ToListAsync();
        }

        public async Task<List<Bid>> GetByTechnicianIdAsync(int technicianId)
        {
            return await _context.Bids
        .Where(b => b.TechnicianId == technicianId)
        .Include(b => b.ServiceRequest)
            .ThenInclude(r => r.Category)
        .Include(b => b.ServiceRequest)
            .ThenInclude(r => r.Profile)
                .ThenInclude(c => c.User)
        .OrderByDescending(b => b.SubmittedAt)
        .ToListAsync();
        }

        public async Task<int> CountByRequestAsync(int requestId)
        {
            return await _context.Bids
                .CountAsync(b => b.ServiceRequestId == requestId
                              && b.Status == BidStatus.pending);
        }

        public async Task<List<Bid>> GetExpiredBidsAsync()
        {
            return await _context.Bids
                .Where(b => b.Status == BidStatus.pending
                         && b.ValidUntil < DateTime.UtcNow)
                .ToListAsync();
        }

        public async Task<bool> HasTechnicianBidAsync(int requestId, int technicianId)
        {
            return await _context.Bids
                .AnyAsync(b => b.ServiceRequestId == requestId
                            && b.TechnicianId == technicianId
                            && b.Status != BidStatus.withdrawn);
        }
        public async Task<List<decimal>> GetAcceptedPricesByCategoryAsync(int categoryId, int limit)
        {
            return await _context.Bids
                .Where(b => b.Status == BidStatus.accepted
                         && b.ServiceRequest.CategoryId == categoryId)
                .OrderByDescending(b => b.SubmittedAt)
                .Take(limit)
                .Select(b => b.Price)
                .ToListAsync();
        }
    }
}
