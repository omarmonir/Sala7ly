using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sala7ly.BLL.DTOs.AddressDTOs;
using Sala7ly.BLL.Mapper;
using Sala7ly.BLL.Services.Abstraction;
using Sala7ly.DAL.Repositories.Abstraction;

namespace Sala7ly.BLL.Services.Implementation
{
    public class AddressService : IAddressService
    {
        private readonly IAddressRepository _addressRepo;
        private readonly ICustomerRepository _customerRepo;

        public AddressService(IAddressRepository addressRepo, ICustomerRepository customerRepo)
        {
            _addressRepo = addressRepo;
            _customerRepo = customerRepo;
        }

        public async Task<IEnumerable<AddressDto>> GetAllByUserIdAsync(string userId)
        {
            var customer = await _customerRepo.GetByUserIdAsync(userId);
            if (customer == null)
                return Enumerable.Empty<AddressDto>();

            var addresses = await _addressRepo.GetAllByCustomerAsync(customer.Id);
            return addresses.Select(AddressMapper.ToDto);
        }

        public async Task<bool> CreateAsync(CreateAddressDto dto)
        {
            var address = AddressMapper.ToEntity(dto);
            address.MarkCreated(dto.CustomerId.ToString());
            await _addressRepo.AddAsync(address);
            return await _addressRepo.SaveChangesAsync();
        }

        public async Task<bool> UpdateAsync(int id, UpdateAddressDto dto)
        {
            var address = await _addressRepo.GetByIdAsync(id);
            if (address is null) return false;

            address.UpdateAddress(dto.Street, dto.City, dto.District);
            address.MarkUpdated(dto.UpdatedBy);
            _addressRepo.Update(address);
            return await _addressRepo.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id, string deletedBy)
        {
            var address = await _addressRepo.GetByIdAsync(id);
            if (address is null) return false;

            address.ToggaleStatus(deletedBy);
            _addressRepo.Update(address);
            return await _addressRepo.SaveChangesAsync();
        }

        public async Task<bool> SetDefaultAsync(int id, int customerId)
        {
            var address = await _addressRepo.GetByIdAsync(id);
            if (address is null || address.CustomerId != customerId) return false;

            var defaults = await _addressRepo.GetDefaultAddressesByCustomerAsync(customerId);
            foreach (var a in defaults)
            {
                a.SetDefault(false);
                _addressRepo.Update(a);
            }

            address.SetDefault(true);
            _addressRepo.Update(address);
            return await _addressRepo.SaveChangesAsync();
        }
    }
}
