using Microsoft.EntityFrameworkCore;
using Sala7ly.DAL.DataBase;
using Sala7ly.DAL.Entities;
using Sala7ly.DAL.Enums;
using Sala7ly.DAL.Repositories.Abstraction;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sala7ly.DAL.Repositories.Implementation
{
    public class ServiceRequestRepository : GenericRepository<ServiceRequest>, IServiceRequestRepository
    {
        public ServiceRequestRepository(AppDbContext context) : base(context)
        {
        }

        public new async Task<ServiceRequest?> GetByIdAsync(int id)
            => await _context.ServiceRequests
                .Include(r => r.Profile)
                .Include(r => r.Category)
                .Include(r => r.Address)
                .FirstOrDefaultAsync(r => r.Id == id && r.IsDeleted != true);

        public async Task<IEnumerable<ServiceRequest>> GetByCustomerIdAsync(int customerId)
            => await _context.ServiceRequests
                .Where(r => r.CustomerId == customerId && r.IsDeleted != true)
                .OrderByDescending(r => r.CreatedOn)
                .ToListAsync();

        public async Task<IEnumerable<ServiceRequest>> GetOpenRequestsAsync()
            => await _context.ServiceRequests
                .Where(r => r.Status == Status.open && r.IsDeleted != true)
                .OrderByDescending(r => r.CreatedOn)
                .ToListAsync();

        public async Task<ServiceRequest?> GetByIdWithPartiesAsync(int requestId)
    => await _context.ServiceRequests
        .Include(r => r.Profile)
            .ThenInclude(p => p.User)
        .Include(r => r.SelectedBid)
            .ThenInclude(b => b.Technician)
                .ThenInclude(t => t.User)
        .FirstOrDefaultAsync(r => r.Id == requestId && r.IsDeleted != true);
    }
}