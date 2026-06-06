using System;

namespace Sala7ly.BLL.Dtos.TechnicianProfile
{
    public class TechnicianProfileResponseDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Bio { get; set; }
        public int ExperienceYears { get; set; }
        public double OverallRating { get; set; }
        public int TotalReviews { get; set; }
        public int CompletedJobs { get; set; }
        public int CancelledJobs { get; set; }
        public string AvgResponseTime { get; set; }
        public bool IsApproved { get; set; }
        public bool IsFeatured { get; set; }
        public string SubscriptionTier { get; set; }
        public DateTime? SubscriptionExpiresAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
    }
}