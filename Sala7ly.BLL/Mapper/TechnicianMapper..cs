using Sala7ly.BLL.DTOs.TechnicianDTOs;
using Sala7ly.DAL.Entities;

namespace Sala7ly.BLL.Mapper
{
    public static class TechnicianMapper
    {
        // dto -> User entity (for Identity creation)
        public static User ToUserEntity(TechnicianRegisterDto dto)
        {
            return new User
            {
                Name = dto.Name,
                Email = dto.Email,
                UserName = dto.Email,   // Identity uses UserName for login
                PhoneNumber = dto.PhoneNumber
            };
        }

        // dto -> TechnicianProfile entity
        public static TechnicianProfile ToProfileEntity(TechnicianRegisterDto dto)
        {
            return new TechnicianProfile
            {
                ExperienceYears = dto.ExperienceYears,
            };
        }

        // apply update dto onto existing User
        public static void ApplyUpdateToUser(TechnicianProfileUpdateDto dto, User user)
        {
            user.Name = dto.Name;
            user.PhoneNumber = dto.PhoneNumber;
            user.ImageUrl = dto.ImageUrl;
        }

        // apply update dto onto existing TechnicianProfile
        public static void ApplyUpdateToProfile(TechnicianProfileUpdateDto dto, TechnicianProfile profile)
        {
            profile.Bio = dto.Bio;
            profile.ExperienceYears = dto.ExperienceYears;
            profile.AvgResponseTime = dto.AvgResponseTime;
        }

        // entity -> details dto
        public static TechnicianProfileDetailsDto ToDetailsDto(TechnicianProfile p)
        {
            return new TechnicianProfileDetailsDto
            {
                Id = p.Id,
                UserId = p.UserId,
                Name = p.User?.Name,
                Email = p.User?.Email,
                PhoneNumber = p.User?.PhoneNumber,
                ImageUrl = p.User?.ImageUrl,
                Bio = p.Bio,
                ExperienceYears = p.ExperienceYears,
                OverallRating = p.OverallRating,
                TotalReviews = p.TotalReviews,
                CompletedJobs = p.CompletedJobs,
                CancelledJobs = p.CancelledJobs,
                AvgResponseTime = p.AvgResponseTime,
                IsApproved = p.IsApproved,
                IsFeatured = p.IsFeatured,
                SubscriptionTier = p.SubscriptionTier.ToString(),
                SubscriptionExpiresAt = p.SubscriptionExpiresAt,
                ApprovedAt = p.ApprovedAt
            };
        }

        // entity -> list item dto
        public static TechnicianListItemDto ToListItemDto(TechnicianProfile p)
        {
            return new TechnicianListItemDto
            {
                Id = p.Id,
                Name = p.User?.Name,
                ImageUrl = p.User?.ImageUrl,
                OverallRating = p.OverallRating,
                CompletedJobs = p.CompletedJobs,
                IsApproved = p.IsApproved,
                IsFeatured = p.IsFeatured,
                SubscriptionTier = p.SubscriptionTier.ToString()
            };
        }
    }
}