namespace Sala7ly.BLL.DTOs.TechnicianDTOs
{
    public class TechnicianRegisterDto
    {
        // user account fields
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public string ImageUrl { get; set; }

        // technician profile fields
        public string Bio { get; set; }
        public int ExperienceYears { get; set; }
        public string AvgResponseTime { get; set; }
    }
}