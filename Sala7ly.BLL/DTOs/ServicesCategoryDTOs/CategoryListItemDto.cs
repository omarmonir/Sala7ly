using System;
using System.Collections.Generic;
using System.Text;

namespace Sala7ly.BLL.DTOs.ServicesCategoryDTOs
{
    public class CategoryListItemDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; }
        public bool IsActive { get; set; }
        public int? ParentCategoryId { get; set; }
        public int SubCategoriesCount { get; set; }
    }


}
