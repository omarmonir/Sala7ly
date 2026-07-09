using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Sala7ly.BLL.DTOs.TechnicianDTOs
{
    public class TechnicianRegisterDto
    {
        // user account fields

        [Required(ErrorMessage = "الاسم مطلوب")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "الاسم يجب أن يكون بين 3 و 50 حرفاً")]
        public string Name { get; set; }

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
        public string Email { get; set; }

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [RegularExpression(@"^01[0125][0-9]{8}$", ErrorMessage = "رقم الهاتف غير صحيح (يجب أن يكون رقم مصري مكوّن من 11 رقماً)")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "كلمة المرور يجب أن تكون 8 أحرف على الأقل")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).+$",
            ErrorMessage = "كلمة المرور يجب أن تحتوي على حرف كبير وحرف صغير ورقم ورمز خاص")]
        public string Password { get; set; }

        public IFormFile? Image { get; set; }

        // technician profile fields

        [Range(0, 60, ErrorMessage = "سنوات الخبرة يجب أن تكون بين 0 و 60")]
        public int ExperienceYears { get; set; }

        [MinLength(1, ErrorMessage = "يجب اختيار تخصص واحد على الأقل")]
        public List<int> CategoryIds { get; set; } = new();
    }
}