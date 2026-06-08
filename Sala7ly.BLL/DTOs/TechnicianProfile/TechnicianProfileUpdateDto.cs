namespace Sala7ly.BLL.DTOs.TechnicianDTOs
{
    public class TechnicianProfileUpdateDto
    {
        // user fields that can change
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string ImageUrl { get; set; }

        // technician profile fields that can change
        public string Bio { get; set; }
        public int ExperienceYears { get; set; }
        public string AvgResponseTime { get; set; }
    }
}