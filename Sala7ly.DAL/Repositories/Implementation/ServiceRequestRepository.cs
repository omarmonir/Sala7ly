using Microsoft.EntityFrameworkCore;
using Sala7ly.DAL.DataBase;
using Sala7ly.DAL.Entities;
using Sala7ly.DAL.Enums;
using Sala7ly.DAL.Repositories.Abstraction;

namespace Sala7ly.DAL.Repositories.Implementation
{
    public class ServiceRequestRepository : GenericRepository<ServiceRequest>, IServiceRequestRepository
    {
        public ServiceRequestRepository(AppDbContext context) : base(context) { }

        // ── GET BY ID (Request Details page) ─────────────────────────────────
        // Already had .Include(r => r.Address) — no change needed here.
        public new async Task<ServiceRequest?> GetByIdAsync(int id)
            => await _context.ServiceRequests
                .Include(r => r.Profile)
                    .ThenInclude(p => p.User)
                .Include(r => r.Category)
                .Include(r => r.Address)
                .FirstOrDefaultAsync(r => r.Id == id && r.IsDeleted != true);

        // ── GET BY CUSTOMER (My Requests page) ───────────────────────────────
        // FIX: was missing ALL .Include() calls — customer name, category, and
        // address were all null in the mapper, so address showed as null/"s".
        public async Task<IEnumerable<ServiceRequest>> GetByCustomerIdAsync(int customerId)
            => await _context.ServiceRequests
                .Include(r => r.Profile)
                    .ThenInclude(p => p.User)
                .Include(r => r.Category)
                .Include(r => r.Address)                // FIX: was missing
                .Where(r => r.CustomerId == customerId && r.IsDeleted != true)
                .OrderByDescending(r => r.CreatedOn)
                .ToListAsync();

        // ── GET OPEN REQUESTS (technician browsing) ───────────────────────────
        // FIX: was missing ALL .Include() calls — same null issue.
        public async Task<IEnumerable<ServiceRequest>> GetOpenRequestsAsync()
            => await _context.ServiceRequests
                .Include(r => r.Profile)
                    .ThenInclude(p => p.User)
                .Include(r => r.Category)
                .Include(r => r.Address)                // FIX: was missing
                .Where(r => r.Status == Status.open && r.IsDeleted != true)
                .OrderByDescending(r => r.CreatedOn)
                .ToListAsync();

        // ── GET ALL (Admin panel) ─────────────────────────────────────────────
        // Already had .Include(r => r.Address) — no change needed here.
        public async Task<IEnumerable<ServiceRequest>> GetAllAsync()
            => await _context.ServiceRequests
                .Include(r => r.Profile)
                    .ThenInclude(p => p.User)
                .Include(r => r.Category)
                .Include(r => r.Address)
                .Where(r => r.IsDeleted != true)
                .OrderByDescending(r => r.CreatedOn)
                .ToListAsync();

        // ── GET BY ID WITH PARTIES (lifecycle: start / complete) ──────────────
        // No address needed here — unchanged.
        public async Task<ServiceRequest?> GetByIdWithPartiesAsync(int requestId)
            => await _context.ServiceRequests
                .Include(r => r.Profile)
                    .ThenInclude(p => p.User)
                .Include(r => r.SelectedBid)
                    .ThenInclude(b => b.Technician)
                        .ThenInclude(t => t.User)
                .Include(r => r.EscrowTransaction)
                .FirstOrDefaultAsync(r => r.Id == requestId && r.IsDeleted != true);

        // ── GET ASSIGNED (Assigned Tasks page) ────────────────────────────────
        // FIX: was missing .Include(r => r.Address) and .Include(r => r.Category)
        // which caused the assigned-tasks list to always show address "s" and
        // a blank category name.
        public async Task<IEnumerable<ServiceRequest>> GetAssignedByTechnicianUserIdAsync(string userId)
            => await _context.ServiceRequests
                .Include(r => r.Profile)
                    .ThenInclude(p => p.User)
                .Include(r => r.Category)               // FIX: was missing
                .Include(r => r.Address)                // FIX: was missing
                .Include(r => r.Bids)
                    .ThenInclude(b => b.Technician)
                        .ThenInclude(t => t.User)
                .Where(r => r.Status == Status.assigned &&
                            r.Bids.Any(b => b.Technician.User.Id == userId) &&
                            r.IsDeleted != true)
                .OrderByDescending(r => r.CreatedOn)
                .ToListAsync();
    }
}