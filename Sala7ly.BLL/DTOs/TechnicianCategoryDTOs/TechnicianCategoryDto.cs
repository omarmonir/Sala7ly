using System;
using System.Collections.Generic;
using System.Text;

namespace Sala7ly.BLL.DTOs.TechnicianCategoryDTOs
{
    public class TechnicianCategoryDto
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int YearsInCategory { get; set; }
        public bool IsPrimary { get; set; }
    }
}
