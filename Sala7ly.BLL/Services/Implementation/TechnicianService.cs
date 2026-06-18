using Microsoft.AspNetCore.Identity;
using Sala7ly.BLL.DTOs.TechnicianDTOs;
using Sala7ly.BLL.Mapper;
using Sala7ly.BLL.Services.Abstraction;
using Sala7ly.DAL.Entities;
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
        private readonly IFileService _fileService;

        public TechnicianService(ITechnicianProfileRepository technicianRepo,
                                UserManager<User> userManager,
                                IFileService fileService)
        {
            _technicianRepo = technicianRepo;
            _userManager = userManager;
            _fileService = fileService;
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

        // Commands

        public async Task<bool> AddAsync(TechnicianRegisterDto dto)
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser is not null) return false;

            // upload image if provided
            string? imageUrl = null;
            if (dto.Image is not null)
                imageUrl = await _fileService.SaveFileAsync(dto.Image, "technicians");

            var user = TechnicianMapper.ToUserEntity(dto);
            if (imageUrl is not null)
                user.SetImageUrl(imageUrl);

            var profile = TechnicianMapper.ToProfileEntity(dto);

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                if (imageUrl is not null)
                    await _fileService.DeleteFileAsync(imageUrl);
                return false;
            }

            profile.UserId = user.Id;
            profile.MarkCreated(user.Id);

            await _technicianRepo.AddAsync(profile);
            var saved = await _technicianRepo.SaveChangesAsync();

            if (saved == 0)
            {
                await _userManager.DeleteAsync(user);
                if (imageUrl is not null)
                    await _fileService.DeleteFileAsync(imageUrl);
                return false;
            }

            await _userManager.AddToRoleAsync(user, "Technician");
            return true;
        }

        public async Task<TechnicianProfileDetailsDto?> GetByUserIdAsync(string userId)
        {
            var profile = await _technicianRepo.GetProfileByUserIdAsync(userId);  // ← match the name
            if (profile is null)
                return null;

            return TechnicianMapper.ToDetailsDto(profile);
        }
        public async Task<bool> UpdateAsync(int id, TechnicianProfileUpdateDto dto)
        {
            // 1 — get profile with user included
            var profile = await _technicianRepo.GetByIdAsync(id);
            if (profile is null)
                return false;

            // 2 — handle image upload
            if (dto.Image is not null)
            {
                var oldImageUrl = profile.User.ImageUrl;

                var newImageUrl = await _fileService.SaveFileAsync(dto.Image, "technicians");
                profile.User.SetImageUrl(newImageUrl);

                if (!string.IsNullOrEmpty(oldImageUrl))
                    await _fileService.DeleteFileAsync(oldImageUrl);
            }

            // 3 — apply changes to both user and profile
            TechnicianMapper.ApplyUpdateToUser(dto, profile.User);
            TechnicianMapper.ApplyUpdateToProfile(dto, profile);
            profile.MarkUpdated(profile.UserId);

            // 4 — persist both
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