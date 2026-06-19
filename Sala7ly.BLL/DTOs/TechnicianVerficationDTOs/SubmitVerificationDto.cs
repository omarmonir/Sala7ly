using Microsoft.AspNetCore.Http;

namespace Sala7ly.BLL.DTOs.VerificationDTOs
{
    public class SubmitVerificationDto
    {
        public int TechnicianId { get; set; }
        public string Bio { get; set; }
        public int ExperienceYears { get; set; }
        public IFormFile FrontImage { get; set; }
        public IFormFile BackImage { get; set; }
    }
}