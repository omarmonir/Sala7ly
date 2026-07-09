using System.Collections.Generic;
using System.Threading.Tasks;
using Sala7ly.DAL.Entities;

namespace Sala7ly.DAL.Repositories.Abstraction
{
    public interface ITechnicianVerificationRepository
    {
        Task<TechnicianVerification?> GetByIdAsync(int id);
        Task<List<TechnicianVerification>> GetByTechnicianIdAsync(int technicianId);
        Task<List<TechnicianVerification>> GetPendingAsync();
        Task AddAsync(TechnicianVerification verification);
        void Update(TechnicianVerification verification);
        Task<int> SaveChangesAsync();
    }
}