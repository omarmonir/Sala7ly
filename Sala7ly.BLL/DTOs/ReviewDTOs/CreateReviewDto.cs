using System.ComponentModel.DataAnnotations;

namespace Sala7ly.BLL.DTOs.ReviewDTOs
{
    public class CreateReviewDto
    {
        [Required]
        public int RequestId { get; set; }

        [Range(1, 5)] public int QualityScore { get; set; }
        [Range(1, 5)] public int PunctualityScore { get; set; }
        [Range(1, 5)] public int CommunicationScore { get; set; }
        [Range(1, 5)] public int ValueScore { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }
    }
}