using Sala7ly.BLL.DTOs.AddressDTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sala7ly.BLL.Services.Abstraction
{
    public interface IAddressService
    {
        Task<IEnumerable<AddressDto>> GetAllAsync(int customerId);
        Task<bool> CreateAsync(CreateAddressDto dto);

        Task<bool> UpdateAsync(int id, UpdateAddressDto dto);
        Task<bool> DeleteAsync(int id, string deletedBy);
        Task<bool> SetDefaultAsync(int id, int customerId);
    }
}
