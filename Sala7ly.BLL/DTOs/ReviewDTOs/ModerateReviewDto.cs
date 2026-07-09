using System.ComponentModel.DataAnnotations;

namespace Sala7ly.BLL.DTOs.ReviewDTOs
{
    public class ModerateReviewDto
    {
        [Required]
        public int ReviewId { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }          

        [MaxLength(500)]
        public string? ModerationNote { get; set; }    
    }
}