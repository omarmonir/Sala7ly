using Sala7ly.BLL.DTOs.ReviewDTOs;
using Sala7ly.DAL.Entities;

namespace Sala7ly.BLL.Mapper
{
    public static class ReviewMapper
    {
        public static ReviewResponseDto ToResponseDto(Review r)
        {
            return new ReviewResponseDto
            {
                Id = r.Id,
                RequestId = r.RequestId,
                ReviewerId = r.ReviewerID,
                ReviewerName = r.Reviewer?.Name,
                RevieweeId = r.RevieweeID,
                RevieweeName = r.Reviewee?.Name,
                QualityScore = r.QualityScore,
                PunctualityScore = r.PunctualityScore,
                CommunicationScore = r.CommunicationScore,
                ValueScore = r.ValueScore,
                OverallScore = r.OverallScore,
                Comment = r.Comment,
                TechnicianReply = r.TechnicianReply,
                CreatedAt = r.CreatedAt
            };
        }
    }
}