using Microsoft.AspNetCore.Http;

namespace Sala7ly.BLL.DTOs.TechnicianDTOs
{
    public class TechnicianProfileUpdateDto
    {
        // user fields that can change
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public IFormFile? ImageUrl { get; set; }

        // technician profile fields that can change
        public string Bio { get; set; }
        public int ExperienceYears { get; set; }
        public int AvgResponseTime { get; set; }
    }
}