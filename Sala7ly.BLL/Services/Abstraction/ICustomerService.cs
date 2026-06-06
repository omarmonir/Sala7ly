using Sala7ly.BLL.DTOs.CustomerDTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sala7ly.BLL.Services.Abstraction
{
    public interface ICustomerService
    {
        Task<CustomerProfileDetailsDto?> GetByIdAsync(int id);
        Task<IEnumerable<CustomerListItemDto>> GetAllAsync();
        Task<bool> AddAsync(CustomerRegisterDto dto);
        Task<bool> UpdateAsync(int id, CustomerProfileUpdateDto dto);
        Task<bool> DeleteAsync(int id, string deletedBy);
    }
}
