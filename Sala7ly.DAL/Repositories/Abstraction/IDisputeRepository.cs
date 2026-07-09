using System.Collections.Generic;
using System.Threading.Tasks;
using Sala7ly.DAL.Entities;

namespace Sala7ly.DAL.Repositories.Abstraction
{
    public interface IDisputeRepository
    {
        Task<Dispute?> GetByIdAsync(int id);
        Task<List<Dispute>> GetAllAsync();
        Task AddAsync(Dispute dispute);
        void Update(Dispute dispute);
        Task<int> SaveChangesAsync();
    }
}
