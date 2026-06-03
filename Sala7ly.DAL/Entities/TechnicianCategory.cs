namespace Sala7ly.DAL.Entities
{
    public class TechnicianCategory // Solution for many-to-many relationship between TechnicianProfile and ServiceCategory
    {
        public int Id { get; set; }

        public int TechnicianId { get; set; }   
        public int CategoryId { get; set; }     

        public int YearsInCategory { get; set; }
        public bool IsPrimary { get; set; } = false;   

        // navigation
        public TechnicianProfile Technician { get; set; }
        public ServiceCategory Category { get; set; }
    }
}