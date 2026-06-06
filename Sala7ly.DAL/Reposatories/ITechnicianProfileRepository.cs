using System.Collections.Generic;
using System.Threading.Tasks;
using Sala7ly.DAL.Entities;

namespace Sala7ly.DAL.Repositories.Interfaces
{
    public interface ITechnicianProfileRepository
    {
        Task<List<TechnicianProfile>> GetAllAsync();
        Task<TechnicianProfile> GetByIdAsync(int id);
        Task<TechnicianProfile> GetByUserIdAsync(string userId);
        Task AddAsync(TechnicianProfile technician);
        void Update(TechnicianProfile technician);
        void Delete(TechnicianProfile technician);
        Task<int> SaveChangesAsync();
    }
}