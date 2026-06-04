using System.Collections.Generic;

namespace Sala7ly.DAL.Entities
{
    public class ServiceCategory : BaseEntity
    {
        public int? ParentCategoryId { get; private set; }   // self-ref for subcategories, nullable
        public string NameAr { get; private set; } // --> name in arabic if we gonna do it in arabic 
        public bool IsActive { get; private set; } = true;


        // navigation
        public ServiceCategory ParentCategory { get; private set; }
        public ICollection<ServiceCategory> SubCategories { get; private set; }
        public ICollection<TechnicianCategory> TechnicianCategories { get; private set; }
        public ICollection<ServiceRequest> ServiceRequests { get; private set; }
    }

}