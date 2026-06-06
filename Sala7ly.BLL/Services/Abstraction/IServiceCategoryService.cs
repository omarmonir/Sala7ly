using Sala7ly.BLL.DTOs.ServicesCategoryDTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sala7ly.BLL.Services.Abstraction
{
    public interface IServiceCategoryService
    {
        Task<CategoryDetailsDto?> GetByIdAsync(int id);
        Task<IEnumerable<CategoryListItemDto>> GetAllAsync();
        Task<bool> AddAsync(CategoryCreateDto dto);
        Task<bool> UpdateAsync(int id, CategoryUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }

}
