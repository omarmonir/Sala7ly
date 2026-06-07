using System.Collections.Generic;
using System.Threading.Tasks;
using Sala7ly.BLL.Dtos.TechnicianPortfolio;

namespace Sala7ly.BLL.Services.Abstraction
{
    public interface ITechnicianPortfolioService
    {
        Task<List<TechnicianPortfolioResponseDto>> GetAllAsync();
        Task<TechnicianPortfolioResponseDto> GetByIdAsync(int id);
        Task<List<TechnicianPortfolioResponseDto>> GetByTechnicianIdAsync(int technicianId);
        Task<TechnicianPortfolioResponseDto> CreateAsync(CreateTechnicianPortfolioDto dto);
        Task<TechnicianPortfolioResponseDto> UpdateAsync(UpdateTechnicianPortfolioDto dto);
        Task<bool> DeleteAsync(int id);
    }
}