using Microsoft.AspNetCore.Identity;
using Sala7ly.BLL.DTOs.CustomerDTOs;
using Sala7ly.BLL.Mapper;
using Sala7ly.BLL.Services.Abstraction;
using Sala7ly.DAL.Entities;
using Sala7ly.DAL.Enums;
using Sala7ly.DAL.Repositories.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sala7ly.BLL.Services.Implementation
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepo;
        private readonly UserManager<User> _userManager;
        private readonly IFileService _fileService;
        private readonly INotificationService _notificationService;
        private readonly IReviewRepository _reviewRepo;

        public CustomerService(ICustomerRepository customerRepo,
                              UserManager<User> userManager,
                              IFileService fileService,
                              INotificationService notificationService,
                              IReviewRepository reviewRepo)
        {
            _customerRepo = customerRepo;
            _userManager = userManager;
            _fileService = fileService;
            _notificationService = notificationService;
            _reviewRepo = reviewRepo;
        }

        // Queries 

        public async Task<CustomerProfileDetailsDto?> GetByIdAsync(int id)
        {
            var profile = await _customerRepo.GetByIdAsync(id);

            if (profile is null)
                return null;

            var dto = CustomerMapper.ToDetailsDto(profile);
            dto.TotalReviews = await CountReviewsAsync(profile.UserId);
            return dto;
        }

        public async Task<IEnumerable<CustomerListItemDto>> GetAllAsync()
        {
            var profiles = await _customerRepo.GetAllAsync();

            return profiles.Select(CustomerMapper.ToListItemDto);
        }

        public async Task<CustomerProfileDetailsDto?> GetMineAsync(string userId)
        {
            var profile = await _customerRepo.GetByUserIdAsync(userId);

            if (profile is null)
                return null;

            var dto = CustomerMapper.ToDetailsDto(profile);
            dto.TotalReviews = await CountReviewsAsync(userId);
            return dto;
        }

        // counts the reviews this customer has written
        private async Task<int> CountReviewsAsync(string userId)
        {
            var reviews = await _reviewRepo.GetByReviewerIdAsync(userId);
            return reviews?.Count ?? 0;
        }

        // ── Commands

        public async Task<bool> AddAsync(CustomerRegisterDto dto)
        {
            // 1 — check email not already taken
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser is not null)
                return false;

            string? imageUrl = null;
            if (dto.Image is not null)
                imageUrl = await _fileService.SaveFileAsync(dto.Image, "customers");

            // 2 — map dto → entities
            var user = CustomerMapper.ToUserEntity(dto);
            if (imageUrl is not null)
                user.SetImageUrl(imageUrl);
            var profile = CustomerMapper.ToProfileEntity();

            // 3 — create user via Identity
            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                if (imageUrl is not null)
                    await _fileService.DeleteFileAsync(imageUrl);
                return false;
            }

            // 4 — link profile and save
            profile.UserId = user.Id;
            profile.MarkCreated(user.Id);

            await _customerRepo.AddAsync(profile);
            var saved = await _customerRepo.SaveChangesAsync();

            // 5 — rollback user if profile save failed 
            if (saved == 0)
            {
                await _userManager.DeleteAsync(user);
                if (imageUrl is not null)
                    await _fileService.DeleteFileAsync(imageUrl);
                return false;
            }
            await _userManager.AddToRoleAsync(user, "Customer");

            // 6 — send a welcome notification
            await _notificationService.NotifyUserAsync(
                userId: user.Id,
                type: NotificationType.system,
                title: "مرحباً بك في صلّحلي 👋",
                body: "سعداء بانضمامك! يمكنك الآن إنشاء طلب جديد والعثور على أفضل الفنيين لخدمتك.",
                deepLink: null);

            return true;
        }

        public async Task<bool> UpdateAsync(int id, CustomerProfileUpdateDto dto)
        {
            // 1 — get profile with user included
            var profile = await _customerRepo.GetByIdAsync(id);
            if (profile is null)
                return false;

            // 2 — handle image upload
            if (dto.Image is not null)
            {
                var oldImageUrl = profile.User.ImageUrl;

                var newImageUrl = await _fileService.SaveFileAsync(dto.Image, "customers");
                profile.User.SetImageUrl(newImageUrl);

                if (!string.IsNullOrEmpty(oldImageUrl))
                    await _fileService.DeleteFileAsync(oldImageUrl);
            }

            // 3 — apply other changes to user entity
            CustomerMapper.ApplyUpdateDto(dto, profile.User);
            profile.MarkUpdated(profile.UserId);

            // 4 — persist both
            await _userManager.UpdateAsync(profile.User);
            _customerRepo.Update(profile);
            await _customerRepo.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(int id, string deletedBy)
        {
            // 1 — get profile with user included
            var profile = await _customerRepo.GetByIdAsync(id);
            if (profile is null)
                return false;

            // 2 — soft delete profile via BaseEntity.ToggaleStatus()
            profile.ToggaleStatus(deletedBy);

            // 3 — deactivate user account
            profile.User.Deactivate();
            await _userManager.UpdateAsync(profile.User);

            // 4 — persist
            _customerRepo.Update(profile);
            await _customerRepo.SaveChangesAsync();

            return true;
        }

    }
}