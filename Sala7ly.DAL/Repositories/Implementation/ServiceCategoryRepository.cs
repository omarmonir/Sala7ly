using Microsoft.EntityFrameworkCore;
using Sala7ly.DAL.DataBase;
using Sala7ly.DAL.Entities;
using Sala7ly.DAL.Repositories.Abstraction;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sala7ly.DAL.Repositories.Implementation
{
    public class ServiceCategoryRepository : GenericRepository<ServiceCategory>,IServiceCategoryRepository
    {
        public ServiceCategoryRepository(AppDbContext context) : base(context) { }

        //  Queries 

        public new async Task<ServiceCategory?> GetByIdAsync(int id)
            => await _context.ServiceCategories
                .Include(c => c.SubCategories)
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

        public new async Task<IEnumerable<ServiceCategory>> GetAllAsync()
              => await _context.ServiceCategories
        .Include(c => c.SubCategories)
        .Where(c => c.IsActive && c.IsDeleted != true) // 
        .ToListAsync();


        public new void Delete(ServiceCategory category)
        {
            category.ToggaleStatus("system");  // sets IsDeleted = true
            category.Deactivate();             //sets IsActive = false
            _context.ServiceCategories.Update(category);
        }



    }
}
