using Microsoft.AspNetCore.Identity;
using Sala7ly.BLL.DTOs.TechnicianDTOs;
using Sala7ly.BLL.Mapper;
using Sala7ly.BLL.Services.Abstraction;
using Sala7ly.DAL.Entities;
using Sala7ly.DAL.Enums;
using Sala7ly.DAL.Repositories.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sala7ly.BLL.Services.Implementation
{
    public class TechnicianService : ITechnicianService
    {
        private readonly ITechnicianProfileRepository _technicianRepo;
        private readonly UserManager<User> _userManager;
        private readonly INotificationService _notificationService;

        public TechnicianService(ITechnicianProfileRepository technicianRepo,
                                 UserManager<User> userManager,
                                 INotificationService notificationService)
        {
            _technicianRepo = technicianRepo;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        // Queries

        public async Task<TechnicianProfileDetailsDto?> GetByIdAsync(int id)
        {
            var profile = await _technicianRepo.GetByIdAsync(id);

            if (profile is null)
                return null;

            return TechnicianMapper.ToDetailsDto(profile);
        }

        public async Task<IEnumerable<TechnicianListItemDto>> GetAllAsync()
        {
            var profiles = await _technicianRepo.GetAllAsync();

            return profiles.Select(TechnicianMapper.ToListItemDto);
        }

        public async Task<TechnicianProfileDetailsDto?> GetByUserIdAsync(string userId)
        {
            var profile = await _technicianRepo.GetByUserIdAsync(userId);

            if (profile is null)
                return null;

            return TechnicianMapper.ToDetailsDto(profile);
        }

        // Commands

        public async Task<bool> AddAsync(TechnicianRegisterDto dto)
        {
            // 1 — check email not already taken
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser is not null)
                return false;

            // 2 — map dto → entities
            var user = TechnicianMapper.ToUserEntity(dto);
            var profile = TechnicianMapper.ToProfileEntity(dto);

            // 3 — create user via Identity
            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return false;

            // 4 — link profile and save
            profile.UserId = user.Id;
            profile.MarkCreated(user.Id);

            await _technicianRepo.AddAsync(profile);
            var saved = await _technicianRepo.SaveChangesAsync();

            // 5 — rollback user if profile save failed
            if (saved == 0)
            {
                await _userManager.DeleteAsync(user);
                return false;
            }

            await _userManager.AddToRoleAsync(user, "Technician");

            // 6 — send a welcome notification nudging them to verify
            await _notificationService.NotifyUserAsync(
                userId: user.Id,
                type: NotificationType.verification,
                title: "مرحباً بك في صلّحلي 👋",
                body: "سعداء بانضمامك! اضغط هنا للانتقال إلى صفحة التوثيق وإكمال توثيق حسابك لتتمكن من استقبال الطلبات.",
                deepLink: null);

            return true;
        }

        public async Task<bool> UpdateAsync(int id, TechnicianProfileUpdateDto dto)
        {
            // 1 — get profile with user included
            var profile = await _technicianRepo.GetByIdAsync(id);
            if (profile is null)
                return false;

            // 2 — apply changes to both user and profile
            TechnicianMapper.ApplyUpdateToUser(dto, profile.User);
            TechnicianMapper.ApplyUpdateToProfile(dto, profile);
            profile.MarkUpdated(profile.UserId);

            // 3 — persist both
            await _userManager.UpdateAsync(profile.User);
            _technicianRepo.Update(profile);
            await _technicianRepo.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(int id, string deletedBy)
        {
            // 1 — get profile with user included
            var profile = await _technicianRepo.GetByIdAsync(id);
            if (profile is null)
                return false;

            // 2 — soft delete profile via BaseEntity
            profile.ToggaleStatus(deletedBy);

            // 3 — deactivate user account
            profile.User.Deactivate();
            await _userManager.UpdateAsync(profile.User);

            // 4 — persist
            _technicianRepo.Update(profile);
            await _technicianRepo.SaveChangesAsync();

            return true;
        }
    }
}