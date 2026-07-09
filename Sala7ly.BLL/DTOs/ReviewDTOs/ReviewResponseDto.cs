using System;

namespace Sala7ly.BLL.DTOs.ReviewDTOs
{
    public class ReviewResponseDto
    {
        public int Id { get; set; }
        public int RequestId { get; set; }

        public string ReviewerId { get; set; }
        public string ReviewerName { get; set; }

        public string RevieweeId { get; set; }
        public string RevieweeName { get; set; }

        public int QualityScore { get; set; }
        public int PunctualityScore { get; set; }
        public int CommunicationScore { get; set; }
        public int ValueScore { get; set; }
        public float OverallScore { get; set; }

        public string? Comment { get; set; }
        public string? TechnicianReply { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}