using Sala7ly.DAL.Entities;

namespace Sala7ly.DAL.Repositories.Abstraction
{
    public interface IEscrowRepository : IGenericRepository<EscrowTransaction>
    {
        Task<EscrowTransaction?> GetByRequestIdAsync(int requestId);
        Task<EscrowTransaction?> GetByProviderRefAsync(string providerRef);
        Task<EscrowTransaction?> GetByIdWithDetailsAsync(int id);
    }
}
