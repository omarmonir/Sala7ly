using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Sala7ly.BLL.Dtos.TechnicianPortfolio;
using Sala7ly.BLL.Services.Abstraction;
using Sala7ly.DAL.Entities;
using Sala7ly.DAL.Repositories.Abstraction;

namespace Sala7ly.BLL.Services.Implementation
{
    public class TechnicianPortfolioService : ITechnicianPortfolioService
    {
        private readonly ITechnicianPortfolioRepository _repository;
        private readonly IFilePathProvider _filePathProvider;

        public TechnicianPortfolioService(
            ITechnicianPortfolioRepository repository,
            IFilePathProvider filePathProvider)
        {
            _repository = repository;
            _filePathProvider = filePathProvider;
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
            var beforeUrl = await SaveImageAsync(dto.BeforeImage);
            if (beforeUrl is null)
                return null;

            var afterUrl = await SaveImageAsync(dto.AfterImage);
            if (afterUrl is null)
                return null;

            var portfolio = new TechnicianPortfolio
            {
                TechnicianId = dto.TechnicianId,
                Title = dto.Title,
                Description = dto.Description,
                ImageUrlBefore = beforeUrl,
                ImageUrlAfter = afterUrl,
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

            if (dto.BeforeImage is not null && dto.BeforeImage.Length > 0)
            {
                var url = await SaveImageAsync(dto.BeforeImage);
                if (url is not null) portfolio.ImageUrlBefore = url;
            }

            if (dto.AfterImage is not null && dto.AfterImage.Length > 0)
            {
                var url = await SaveImageAsync(dto.AfterImage);
                if (url is not null) portfolio.ImageUrlAfter = url;
            }

            portfolio.Title = dto.Title;
            portfolio.Description = dto.Description;

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

        private async Task<string?> SaveImageAsync(IFormFile file)
        {
            if (file is null || file.Length == 0)
                return null;

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return null;

            const long maxSize = 5 * 1024 * 1024;
            if (file.Length > maxSize)
                return null;

            var webRoot = _filePathProvider.GetWebRootPath();
            var uploadsFolder = Path.Combine(webRoot, "uploads", "portfolio");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/portfolio/{fileName}";
        }

        private static TechnicianPortfolioResponseDto MapToResponse(TechnicianPortfolio p)
        {
            return new TechnicianPortfolioResponseDto
            {
                Id = p.Id,
                TechnicianId = p.TechnicianId,
                Title = p.Title,
                Description = p.Description,
                ImageUrlBefore = p.ImageUrlBefore,
                ImageUrlAfter = p.ImageUrlAfter,
                UploadedAt = p.UploadedAt
            };
        }
    }
}