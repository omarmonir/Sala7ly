using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sala7ly.DAL.DataBase;
using Sala7ly.DAL.Entities;
using Sala7ly.DAL.Repositories.Interfaces;

namespace Sala7ly.DAL.Repositories
{
    public class TechnicianPortfolioRepository : ITechnicianPortfolioRepository
    {
        private readonly AppDbContext _context;

        public TechnicianPortfolioRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<TechnicianPortfolio>> GetAllAsync()
        {
            return await _context.TechnicianPortfolios.ToListAsync();
        }

        public async Task<TechnicianPortfolio> GetByIdAsync(int id)
        {
            return await _context.TechnicianPortfolios.FindAsync(id);
        }

        public async Task<List<TechnicianPortfolio>> GetByTechnicianIdAsync(int technicianId)
        {
            return await _context.TechnicianPortfolios
                .Where(p => p.TechnicianId == technicianId)
                .ToListAsync();
        }

        public async Task AddAsync(TechnicianPortfolio portfolio)
        {
            await _context.TechnicianPortfolios.AddAsync(portfolio);
        }

        public void Update(TechnicianPortfolio portfolio)
        {
            _context.TechnicianPortfolios.Update(portfolio);
        }

        public void Delete(TechnicianPortfolio portfolio)
        {
            _context.TechnicianPortfolios.Remove(portfolio);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}