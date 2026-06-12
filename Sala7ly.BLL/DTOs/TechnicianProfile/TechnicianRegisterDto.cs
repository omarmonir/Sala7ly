using Microsoft.AspNetCore.Http;

namespace Sala7ly.BLL.DTOs.TechnicianDTOs
{
    public class TechnicianRegisterDto
    {
        // user account fields
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public IFormFile? Image { get; set; }

        // technician profile fields
        public int ExperienceYears { get; set; }
    }
}