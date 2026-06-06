using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Sala7ly.BLL.DTOs.ServicesCategoryDTOs
{
    public class CategoryCreateDto
    {
        [Required(ErrorMessage = "اسم الفئة مطلوب")]
        [MaxLength(100, ErrorMessage = "الاسم لا يتجاوز 100 حرف")]
        public string NameAr { get; set; }

        public int? ParentCategoryId { get; set; }
    }

}
