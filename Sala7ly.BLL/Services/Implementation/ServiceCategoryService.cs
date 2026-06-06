using Sala7ly.BLL.DTOs.ServicesCategoryDTOs;
using Sala7ly.BLL.Mapper;
using Sala7ly.BLL.Services.Abstraction;
using Sala7ly.DAL.Repositories.Abstraction;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sala7ly.BLL.Services.Implementation
{
    public class ServiceCategoryService : IServiceCategoryService
    {
        private readonly IServiceCategoryRepository _categoryRepo;

        public ServiceCategoryService(IServiceCategoryRepository categoryRepo)
        {
            _categoryRepo = categoryRepo;
        }


        // ── Queries 

        public async Task<CategoryDetailsDto?> GetByIdAsync(int id)
        {
            var category = await _categoryRepo.GetByIdAsync(id);

            if (category is null)
                return null;

            return CategoryMapper.ToDetailsDto(category);
        }

        public async Task<IEnumerable<CategoryListItemDto>> GetAllAsync()
        {
            var categories = await _categoryRepo.GetAllAsync();
            return categories.Select(CategoryMapper.ToListItemDto);
        }




        // ── Commands 

        public async Task<bool> AddAsync(CategoryCreateDto dto)
        {
            var category = CategoryMapper.ToEntity(dto);
            category.MarkCreated("system");

            await _categoryRepo.AddAsync(category);
            await _categoryRepo.SaveChangesAsync();

            return true;
        }


        public async Task<bool> UpdateAsync(int id, CategoryUpdateDto dto)
        {
            var category = await _categoryRepo.GetByIdAsync(id);
            if (category is null)
                return false;

            CategoryMapper.ApplyUpdateDto(dto, category);
            category.MarkUpdated("system");

            _categoryRepo.Update(category);
            await _categoryRepo.SaveChangesAsync();

            return true;
        }


        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _categoryRepo.GetByIdAsync(id);
            if (category is null)
                return false;

            _categoryRepo.Delete(category);
            await _categoryRepo.SaveChangesAsync();

            return true;
        }



    }
}
