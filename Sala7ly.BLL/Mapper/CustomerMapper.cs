using Sala7ly.BLL.DTOs.CustomerDTOs;
using Sala7ly.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sala7ly.BLL.Mapper
{
    public static class CustomerMapper
    {
        // ── Entity → DTO 

        public static CustomerListItemDto ToListItemDto(CustomerProfile profile)=> new CustomerListItemDto
        {
                Id = profile.Id,
                Name = profile.User.Name,
                Email = profile.User.Email,
                RegisteredAt = profile.CreatedOn,
                TotalRequests = profile.TotalRequests,
                IsActive = profile.User.IsActive
        };

        public static CustomerProfileDetailsDto ToDetailsDto(CustomerProfile profile)=> new CustomerProfileDetailsDto
        {
                Id = profile.Id,
                Name = profile.User.Name,
                Email = profile.User.Email,
                PhoneNumber = profile.User.PhoneNumber,
                ImageUrl = profile.User.ImageUrl,
                MainAddress = profile.Addresses?.FirstOrDefault()?.Street ?? string.Empty,
                TotalRequests = profile.TotalRequests,
                TotalReviews = profile.TotalReviews,
                MemberSinceYear = profile.CreatedOn.HasValue? profile.CreatedOn.Value.Year: DateTime.UtcNow.Year
        };

        // ── DTO → Entity 
        public static User ToUserEntity(CustomerRegisterDto dto) => new User
        {
            Name = dto.Name,
            Email = dto.Email,
            UserName = dto.Email,
            NormalizedEmail = dto.Email.ToUpper(),   // add
            NormalizedUserName = dto.Email.ToUpper()    // add
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
            user.SetImageUrl(dto.ImageUrl);
        }



    }
}
