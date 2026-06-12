using Sala7ly.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sala7ly.DAL.Repositories.Abstraction
{
    public interface IAddressRepository : IGenericRepository<Address>
    {
        Task<IEnumerable<Address>> GetAllByCustomerAsync(int  customerId);
        Task<Address> GetByIdAsync(int customerId);
        Task AddAsync(Address address);
        void Update(Address address);
        void Delete (Address address);
        Task<bool> SaveChangesAsync();
        Task<IEnumerable<Address>> GetDefaultAddressesByCustomerAsync(int customerId);
    }
}
