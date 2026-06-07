using Sala7ly.BLL.DTOs.ServicesCategoryDTOs;
using Sala7ly.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sala7ly.BLL.Mapper
{

    public static class CategoryMapper
    {
        // ── Entity → DTO

        public static CategoryListItemDto ToListItemDto(ServiceCategory category)
            => new CategoryListItemDto
            {
                Id = category.Id,
                NameAr = category.NameAr,
                IsActive = category.IsActive,
                ParentCategoryId = category.ParentCategoryId,
                SubCategoriesCount = category.SubCategories?.Count() ?? 0
            };

        public static CategoryDetailsDto ToDetailsDto(ServiceCategory category)
            => new CategoryDetailsDto
            {
                Id = category.Id,
                NameAr = category.NameAr,
                IsActive = category.IsActive,
                ParentCategoryId = category.ParentCategoryId,
                ParentCategoryName = category.ParentCategory?.NameAr ?? string.Empty,
                SubCategories = category.SubCategories?
                                         .Select(ToListItemDto) ?? Enumerable.Empty<CategoryListItemDto>()
            };

        // ── DTO → Entity 


        public static ServiceCategory ToEntity(CategoryCreateDto dto)
            => new ServiceCategory(        //  uses constructor
                nameAr: dto.NameAr,
                parentCategoryId: dto.ParentCategoryId
            );

        public static void ApplyUpdateDto(CategoryUpdateDto dto, ServiceCategory category)
            => category.Update(            //  uses entity method
                nameAr: dto.NameAr,
                parentCategoryId: dto.ParentCategoryId
            );

    }


}
