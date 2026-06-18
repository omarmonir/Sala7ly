using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sala7ly.BLL.DTOs.VerificationDTOs
{
    public class SubmitVerificationDto
    {
        public string Bio { get; set; }
        public int ExperienceYears { get; set; }
        [Required(ErrorMessage = "الرقم القومي مطلوب")]
        [RegularExpression(@"^\d{14}$", ErrorMessage = "الرقم القومي يجب أن يتكون من 14 رقم")]
        public string IdNumber { get; set; }
        public IFormFile FrontImage { get; set; }
        public IFormFile BackImage { get; set; }
        public List<IFormFile>? DegreeCertificates { get; set; }   
    }
}