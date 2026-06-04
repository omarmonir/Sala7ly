namespace Sala7ly.DAL.Entities
{
    public class TechnicianCategory : BaseEntity // Solution for many-to-many relationship between TechnicianProfile and ServiceCategory
    {
        public int TechnicianId { get; private set; }   
        public int CategoryId { get; private set; }     

        public int YearsInCategory { get; private set; }
        public bool IsPrimary { get; private set; } = false;   

        // navigation
        public TechnicianProfile Technician { get; private set; }
        public ServiceCategory Category { get; private set; }
    }
}