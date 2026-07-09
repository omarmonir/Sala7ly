using Sala7ly.BLL.DTOs.CustomerDTOs;
using Sala7ly.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sala7ly.BLL.Mapper
{
    public static class CustomerMapper
    {
        // ── Entity → DTO 

        public static CustomerListItemDto ToListItemDto(CustomerProfile profile) => new CustomerListItemDto
        {
            Id = profile.Id,
            Name = profile.User.Name,
            Email = profile.User.Email,
            RegisteredAt = profile.CreatedOn,
            TotalRequests = profile.ServiceRequests?.Count(r => r.IsDeleted != true) ?? 0,
            IsActive = profile.User.IsActive,
            ImageUrl = profile.User.ImageUrl
        };

        public static CustomerProfileDetailsDto ToDetailsDto(CustomerProfile profile)
        {
            // prefer the default address, otherwise the first one
            var address = profile.Addresses?.FirstOrDefault(a => a.IsDefault)
                       ?? profile.Addresses?.FirstOrDefault();

            // build a readable address: "Street، District، City"
            var addressParts = new[] { address?.Street, address?.District, address?.City }
                .Where(p => !string.IsNullOrWhiteSpace(p));

            return new CustomerProfileDetailsDto
            {
                Id = profile.Id,
                Name = profile.User.Name,
                Email = profile.User.Email,
                PhoneNumber = profile.User.PhoneNumber,
                ImageUrl = profile.User.ImageUrl,

                MainAddress = addressParts.Any()
                    ? string.Join("، ", addressParts)
                    : string.Empty,

                // counted live from the loaded navigation, not the stale column
                TotalRequests = profile.ServiceRequests?.Count(r => r.IsDeleted != true) ?? 0,

                // set by the service (reviews link to User, not CustomerProfile)
                TotalReviews = profile.TotalReviews,

                MemberSinceYear = profile.CreatedOn.HasValue
                    ? profile.CreatedOn.Value.Year
                    : DateTime.UtcNow.Year
            };
        }

        // ── DTO → Entity 
        public static User ToUserEntity(CustomerRegisterDto dto) => new User
        {
            Name = dto.Name,
            Email = dto.Email,
            UserName = dto.Email,
            NormalizedEmail = dto.Email.ToUpper(),
            NormalizedUserName = dto.Email.ToUpper()
        };

        public static CustomerProfile ToProfileEntity() => new CustomerProfile
        {
            TotalRequests = 0,
            TotalReviews = 0,
            TotalSpent = 0
        };

        public static void ApplyUpdateDto(CustomerProfileUpdateDto dto, User user)
        {
            user.Name = dto.Name;
            user.PhoneNumber = dto.PhoneNumber;
        }

    }
}