using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sala7ly.BLL.Dtos.TechnicianPortfolio;
using Sala7ly.BLL.Services.Abstraction;
using Sala7ly.DAL.Entities;
using Sala7ly.DAL.Repositories.Abstraction;

namespace Sala7ly.BLL.Services.Implementation
{
    public class TechnicianPortfolioService : ITechnicianPortfolioService
    {
        private readonly ITechnicianPortfolioRepository _repository;

        public TechnicianPortfolioService(ITechnicianPortfolioRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<TechnicianPortfolioResponseDto>> GetAllAsync()
        {
            var items = await _repository.GetAllAsync();
            return items.Select(MapToResponse).ToList();
        }

        public async Task<TechnicianPortfolioResponseDto> GetByIdAsync(int id)
        {
            var item = await _repository.GetByIdAsync(id);
            return item == null ? null : MapToResponse(item);
        }

        public async Task<List<TechnicianPortfolioResponseDto>> GetByTechnicianIdAsync(int technicianId)
        {
            var items = await _repository.GetByTechnicianIdAsync(technicianId);
            return items.Select(MapToResponse).ToList();
        }

        public async Task<TechnicianPortfolioResponseDto> CreateAsync(CreateTechnicianPortfolioDto dto)
        {
            var portfolio = new TechnicianPortfolio
            {
                TechnicianId = dto.TechnicianId,
                ServiceRequestId = dto.ServiceRequestId,
                ImageUrl = dto.ImageUrl,
                Caption = dto.Caption,
                Type = dto.Type,
                UploadedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(portfolio);
            await _repository.SaveChangesAsync();

            return MapToResponse(portfolio);
        }

        public async Task<TechnicianPortfolioResponseDto> UpdateAsync(UpdateTechnicianPortfolioDto dto)
        {
            var portfolio = await _repository.GetByIdAsync(dto.Id);
            if (portfolio == null)
                return null;

            portfolio.ImageUrl = dto.ImageUrl;
            portfolio.Caption = dto.Caption;
            portfolio.Type = dto.Type;

            _repository.Update(portfolio);
            await _repository.SaveChangesAsync();

            return MapToResponse(portfolio);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var portfolio = await _repository.GetByIdAsync(id);
            if (portfolio == null)
                return false;

            _repository.Delete(portfolio);
            await _repository.SaveChangesAsync();
            return true;
        }

        private static TechnicianPortfolioResponseDto MapToResponse(TechnicianPortfolio p)
        {
            return new TechnicianPortfolioResponseDto
            {
                Id = p.Id,
                TechnicianId = p.TechnicianId,
                ServiceRequestId = p.ServiceRequestId,
                ImageUrl = p.ImageUrl,
                Caption = p.Caption,
                Type = p.Type,
                UploadedAt = p.UploadedAt
            };
        }
    }
}