using System.Collections.Generic;
using System.Threading.Tasks;
using Sala7ly.DAL.Entities;

namespace Sala7ly.DAL.Repositories.Abstraction
{
    public interface ITechnicianProfileRepository
    {
        Task<List<TechnicianProfile>> GetAllAsync();
        Task<TechnicianProfile> GetByIdAsync(int id);
        Task<TechnicianProfile> GetByUserIdAsync(string userId);
        Task<TechnicianProfile?> GetProfileByUserIdAsync(string userId);

         
        Task<List<int>> GetCategoryIdsByUserIdAsync(string userId);

     
        Task<List<string>> GetUserIdsByAnyCategoryAsync(IEnumerable<int> categoryIds);

        Task AddAsync(TechnicianProfile technician);
        void Update(TechnicianProfile technician);
        void Delete(TechnicianProfile technician);
        Task<int> SaveChangesAsync();
    }
}