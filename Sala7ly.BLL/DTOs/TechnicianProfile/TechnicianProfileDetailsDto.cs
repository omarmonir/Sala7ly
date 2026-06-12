using System;

namespace Sala7ly.BLL.DTOs.TechnicianDTOs
{
    public class TechnicianProfileDetailsDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }

        // from User
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string ImageUrl { get; set; }

        // from TechnicianProfile
        public string Bio { get; set; }
        public int ExperienceYears { get; set; }
        public double OverallRating { get; set; }
        public int TotalReviews { get; set; }
        public int CompletedJobs { get; set; }
        public int CancelledJobs { get; set; }
        public int AvgResponseTime { get; set; }
        public bool IsApproved { get; set; }
        public bool IsFeatured { get; set; }
        public string SubscriptionTier { get; set; }
        public DateTime? SubscriptionExpiresAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
    }
}