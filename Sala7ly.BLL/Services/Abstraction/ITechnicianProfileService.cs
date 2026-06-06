using System.Collections.Generic;
using System.Threading.Tasks;
using Sala7ly.BLL.Dtos.TechnicianProfile;

namespace Sala7ly.BLL.Services.Abstraction
{
    public interface ITechnicianProfileService
    {
        Task<List<TechnicianProfileResponseDto>> GetAllAsync();
        Task<TechnicianProfileResponseDto> GetByIdAsync(int id);
        Task<TechnicianProfileResponseDto> CreateAsync(CreateTechnicianProfileDto dto);
        Task<TechnicianProfileResponseDto> UpdateAsync(UpdateTechnicianProfileDto dto);
        Task<bool> DeleteAsync(int id);
    }
}