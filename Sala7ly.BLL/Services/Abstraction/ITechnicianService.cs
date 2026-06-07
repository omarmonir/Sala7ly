using Sala7ly.BLL.DTOs.TechnicianDTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sala7ly.BLL.Services.Abstraction
{
    public interface ITechnicianService
    {
        Task<TechnicianProfileDetailsDto?> GetByIdAsync(int id);
        Task<IEnumerable<TechnicianListItemDto>> GetAllAsync();
        Task<bool> AddAsync(TechnicianRegisterDto dto);
        Task<bool> UpdateAsync(int id, TechnicianProfileUpdateDto dto);
        Task<bool> DeleteAsync(int id, string deletedBy);
    }
}