using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sala7ly.DAL.DataBase;
using Sala7ly.DAL.Entities;
using Sala7ly.DAL.Repositories.Abstraction;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sala7ly.DAL.Repositories.Implementation
{
    public class CustomerRepository : GenericRepository<CustomerProfile>, ICustomerRepository
    {
        private readonly UserManager<User> _userManager;

        public CustomerRepository(AppDbContext context, UserManager<User> userManager) : base(context)  
        {
            _userManager = userManager;
        }

       
        public new async Task<CustomerProfile?> GetByIdAsync(int id)
            => await _context.CustomerProfiles
                .Include(cp => cp.User)
                .Include(cp => cp.Addresses)
                .Include(cp => cp.ServiceRequests)
                .Include(cp => cp.FavoriteTechnicians)
                .FirstOrDefaultAsync(cp => cp.Id == id && cp.IsDeleted != true);

        public new async Task<IEnumerable<CustomerProfile>> GetAllAsync()
            => await _context.CustomerProfiles
                .Include(cp => cp.User)
                .Where(cp => cp.IsDeleted != true)
                .ToListAsync();

        public new async Task AddAsync(CustomerProfile profile)
            => await _context.CustomerProfiles.AddAsync(profile);

        public new void Delete(CustomerProfile profile)
        {
            profile.ToggaleStatus(profile.UserId);
            profile.User.IsActive = false;
            _context.CustomerProfiles.Update(profile);
        }



    }
}
