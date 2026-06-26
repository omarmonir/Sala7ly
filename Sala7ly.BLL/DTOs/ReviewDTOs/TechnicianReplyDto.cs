using System.ComponentModel.DataAnnotations;

namespace Sala7ly.BLL.DTOs.ReviewDTOs
{
    public class TechnicianReplyDto
    {
        [Required]
        public int ReviewId { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Reply { get; set; }
    }
}