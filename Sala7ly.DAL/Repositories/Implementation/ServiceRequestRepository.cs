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
        public new async Task<ServiceRequest?> GetByIdAsync(int id)
            => await _context.ServiceRequests
                .Include(r => r.Profile)
                    .ThenInclude(p => p.User)
                .Include(r => r.Category)
                .Include(r => r.Address)
                .FirstOrDefaultAsync(r => r.Id == id && r.IsDeleted != true);

        // ── GET BY CUSTOMER (My Requests page) ───────────────────────────────
        public async Task<IEnumerable<ServiceRequest>> GetByCustomerIdAsync(int customerId)
            => await _context.ServiceRequests
                .Include(r => r.Profile)
                    .ThenInclude(p => p.User)
                .Include(r => r.Category)
                .Include(r => r.Address)
                .Where(r => r.CustomerId == customerId && r.IsDeleted != true)
                .OrderByDescending(r => r.CreatedOn)
                .ToListAsync();
        public async Task<ServiceRequest?> GetByIdWithDetailsAsync(int requestId)
        {
            return await _context.ServiceRequests
                .Include(r => r.Category)
                .Include(r => r.Address)
                .Include(r => r.Profile)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(r => r.Id == requestId && r.IsDeleted != true);
        }

        // ── GET ALL OPEN REQUESTS (Admin / unfiltered fallback) ───────────────
        public async Task<IEnumerable<ServiceRequest>> GetOpenRequestsAsync()
            => await _context.ServiceRequests
                .Include(r => r.Profile)
                    .ThenInclude(p => p.User)
                .Include(r => r.Category)
                .Include(r => r.Address)
                .Where(r => r.Status == Status.open && r.IsDeleted != true)
                .OrderByDescending(r => r.CreatedOn)
                .ToListAsync();

        // ── GET OPEN REQUESTS FILTERED BY CATEGORY (Technician feed) ──────────
       
        public async Task<IEnumerable<ServiceRequest>> GetOpenRequestsByCategoryIdsAsync(
            IEnumerable<int> categoryIds)
        {
            var ids = categoryIds.ToList();    

            return await _context.ServiceRequests
                .Include(r => r.Profile)
                    .ThenInclude(p => p.User)
                .Include(r => r.Category)
                .Include(r => r.Address)
                .Where(r => r.Status == Status.open
                         && r.IsDeleted != true
                         && ids.Contains(r.CategoryId))
                .OrderByDescending(r => r.CreatedOn)
                .ToListAsync();
        }

        // ── GET ALL (Admin panel) ─────────────────────────────────────────────
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
        public async Task<IEnumerable<ServiceRequest>> GetAssignedByTechnicianUserIdAsync(string userId)
            => await _context.ServiceRequests
                .Include(r => r.Profile)
                    .ThenInclude(p => p.User)
                .Include(r => r.Category)
                .Include(r => r.Address)
                .Include(r => r.Bids)
                    .ThenInclude(b => b.Technician)
                        .ThenInclude(t => t.User)
                .Where(r => r.Status != Status.in_progress 
                         && r.Status != Status.open
                         && r.Bids.Any(b => b.Technician.User.Id == userId)
                         && r.IsDeleted != true)
                .OrderByDescending(r => r.CreatedOn)
                .ToListAsync();


        // ── GET COMPLETED BY CATEGORY (RAG context for AI endpoints) ─────────
        public async Task<IEnumerable<ServiceRequest>> GetCompletedByCategoryAsync(
            int categoryId, int limit = 20)
            => await _context.ServiceRequests
                .Include(r => r.Category)
                .Include(r => r.SelectedBid)
                .Where(r => r.CategoryId == categoryId
                         && r.Status == Status.completed
                         && r.IsDeleted != true)
                .OrderByDescending(r => r.CompletedAt)
                .Take(limit)
                .ToListAsync();

    }
}