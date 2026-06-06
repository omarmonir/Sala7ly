using System.Collections.Generic;
using System.Threading.Tasks;
using Sala7ly.DAL.Entities;

namespace Sala7ly.DAL.Repositories.Interfaces
{
    public interface ITechnicianPortfolioRepository
    {
        Task<List<TechnicianPortfolio>> GetAllAsync();
        Task<TechnicianPortfolio> GetByIdAsync(int id);
        Task<List<TechnicianPortfolio>> GetByTechnicianIdAsync(int technicianId);
        Task AddAsync(TechnicianPortfolio portfolio);
        void Update(TechnicianPortfolio portfolio);
        void Delete(TechnicianPortfolio portfolio);
        Task<int> SaveChangesAsync();
    }
}