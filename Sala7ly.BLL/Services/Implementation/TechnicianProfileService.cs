using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sala7ly.BLL.Dtos.TechnicianProfile;
using Sala7ly.BLL.Services.Abstraction;
using Sala7ly.DAL.Entities;
using Sala7ly.DAL.Repositories.Abstraction;

namespace Sala7ly.BLL.Services.Implementation
{
    public class TechnicianProfileService : ITechnicianProfileService
    {
        private readonly ITechnicianProfileRepository _repository;

        public TechnicianProfileService(ITechnicianProfileRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<TechnicianProfileResponseDto>> GetAllAsync()
        {
            var technicians = await _repository.GetAllAsync();
            return technicians.Select(MapToResponse).ToList();
        }

        public async Task<TechnicianProfileResponseDto> GetByIdAsync(int id)
        {
            var technician = await _repository.GetByIdAsync(id);
            return technician == null ? null : MapToResponse(technician);
        }

        public async Task<TechnicianProfileResponseDto> CreateAsync(CreateTechnicianProfileDto dto)
        {
            var technician = new TechnicianProfile
            {
                UserId = dto.UserId,
                Bio = dto.Bio,
                ExperienceYears = dto.ExperienceYears,
                AvgResponseTime = dto.AvgResponseTime
            };

            await _repository.AddAsync(technician);
            await _repository.SaveChangesAsync();

            return MapToResponse(technician);
        }

        public async Task<TechnicianProfileResponseDto> UpdateAsync(UpdateTechnicianProfileDto dto)
        {
            var technician = await _repository.GetByIdAsync(dto.Id);
            if (technician == null)
                return null;

            technician.Bio = dto.Bio;
            technician.ExperienceYears = dto.ExperienceYears;
            technician.AvgResponseTime = dto.AvgResponseTime;

            _repository.Update(technician);
            await _repository.SaveChangesAsync();

            return MapToResponse(technician);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var technician = await _repository.GetByIdAsync(id);
            if (technician == null)
                return false;

            _repository.Delete(technician);
            await _repository.SaveChangesAsync();
            return true;
        }

        private static TechnicianProfileResponseDto MapToResponse(TechnicianProfile t)
        {
            return new TechnicianProfileResponseDto
            {
                Id = t.Id,
                UserId = t.UserId,
                Bio = t.Bio,
                ExperienceYears = t.ExperienceYears,
                OverallRating = t.OverallRating,
                TotalReviews = t.TotalReviews,
                CompletedJobs = t.CompletedJobs,
                CancelledJobs = t.CancelledJobs,
                AvgResponseTime = t.AvgResponseTime,
                IsApproved = t.IsApproved,
                IsFeatured = t.IsFeatured,
                SubscriptionTier = t.SubscriptionTier.ToString(),
                SubscriptionExpiresAt = t.SubscriptionExpiresAt,
                ApprovedAt = t.ApprovedAt
            };
        }
    }
}