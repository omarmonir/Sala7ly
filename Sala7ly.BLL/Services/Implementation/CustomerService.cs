using Microsoft.AspNetCore.Identity;
using Sala7ly.BLL.DTOs.CustomerDTOs;
using Sala7ly.BLL.Mapper;
using Sala7ly.BLL.Services.Abstraction;
using Sala7ly.DAL.Entities;
using Sala7ly.DAL.Repositories.Abstraction;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sala7ly.BLL.Services.Implementation
{
    public class CustomerService : ICustomerService
    {

        private readonly ICustomerRepository _customerRepo;
        private readonly UserManager<User> _userManager;

        public CustomerService(ICustomerRepository customerRepo,
                               UserManager<User> userManager)
        {
            _customerRepo = customerRepo;
            _userManager = userManager;
        }

        // Queries 

        public async Task<CustomerProfileDetailsDto?> GetByIdAsync(int id)
        {
            var profile = await _customerRepo.GetByIdAsync(id);

            if (profile is null)
                return null;

            return CustomerMapper.ToDetailsDto(profile);
        }

        public async Task<IEnumerable<CustomerListItemDto>> GetAllAsync()
        {
            var profiles = await _customerRepo.GetAllAsync();

            return profiles.Select(CustomerMapper.ToListItemDto);
        }

        // ── Commands

        public async Task<bool> AddAsync(CustomerRegisterDto dto)
        {
            // 1 — check email not already taken
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser is not null)
                return false;

            // 2 — map dto → entities
            var user = CustomerMapper.ToUserEntity(dto);
            var profile = CustomerMapper.ToProfileEntity();


            // 3 — create user via Identity
            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return false; // back to this



            // 4 — link profile and save
            profile.UserId = user.Id;
            profile.MarkCreated(user.Id);

            await _customerRepo.AddAsync(profile);
            var saved = await _customerRepo.SaveChangesAsync();

            // 5 — rollback user if profile save failed 
            if (saved == 0)
            {
                await _userManager.DeleteAsync(user);
                return false;
            }
            await _userManager.AddToRoleAsync(user, "Customer");

            return true;
        }


        public async Task<bool> UpdateAsync(int id, CustomerProfileUpdateDto dto)
        {
            // 1 — get profile with user included
            var profile = await _customerRepo.GetByIdAsync(id);
            if (profile is null)
                return false;

            // 2 — apply changes to user entity
            CustomerMapper.ApplyUpdateDto(dto, profile.User);
            profile.MarkUpdated(profile.UserId);

            // 3 — persist both
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
