using System.Collections.Generic;

namespace Sala7ly.DAL.Entities
{
    public class ServiceCategory
    {
        public int Id { get; set; }

        public int? ParentCategoryId { get; set; }   // self-ref for subcategories, nullable
        public string NameAr { get; set; } // --> name in arabic if we gonna do it in arabic 
        public bool IsActive { get; set; } = true;












        // navigation
        public ServiceCategory ParentCategory { get; set; }
        public ICollection<ServiceCategory> SubCategories { get; set; }
        public ICollection<TechnicianCategory> TechnicianCategories { get; set; }
    }
}