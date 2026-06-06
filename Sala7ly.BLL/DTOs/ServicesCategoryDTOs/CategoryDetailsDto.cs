using System;
using System.Collections.Generic;
using System.Text;

namespace Sala7ly.BLL.DTOs.ServicesCategoryDTOs
{
    public class CategoryDetailsDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; }
        public bool IsActive { get; set; }
        public int? ParentCategoryId { get; set; }
        public string ParentCategoryName { get; set; }
        public IEnumerable<CategoryListItemDto> SubCategories { get; set; }
    }

}
