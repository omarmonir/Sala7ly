using Microsoft.EntityFrameworkCore;
using Sala7ly.DAL.DataBase;
using Sala7ly.DAL.Entities;
using Sala7ly.DAL.Repositories.Abstraction;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sala7ly.DAL.Repositories.Implementation
{
    public class AddressRepository :GenericRepository<Address>, IAddressRepository
    {
        private readonly AppDbContext _context;
        public AddressRepository(AppDbContext context) : base(context)
        {
            {
            _context = context;
        }}

        public async Task<IEnumerable<Address>> GetAllByCustomerAsync(int customerId)
        {
            return await _context.Addresses
                .Where(a => a.CustomerId == customerId && (a.IsDeleted == null || a.IsDeleted == false))
                .ToListAsync();
        }



        public async Task<Address> GetByIdAsync(int id)
        {
            return await _context.Addresses
                .FirstOrDefaultAsync(a => a.Id == id && (a.IsDeleted == null || a.IsDeleted == false));
        }



        public async Task AddAsync(Address address)
        {
            await _context.Addresses.AddAsync(address);
        }

        public void Update(Address address)
        {
            _context.Addresses.Update(address);
        }

        public void Delete(Address address)
        {
            // بيستخدم ToggaleStatus من BaseEntity زي باقي الـ entities في البروجكت
            address.ToggaleStatus(address.DeletedBy ?? "system");
            _context.Addresses.Update(address);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<Address>> GetDefaultAddressesByCustomerAsync(int customerId)
        {
            return await _context.Addresses
                .Where(a => a.CustomerId == customerId
                         && a.IsDefault == true
                         && (a.IsDeleted == null || a.IsDeleted == false))
                .ToListAsync();
        }

    }
}
