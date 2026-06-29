using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Sala7ly.DAL.Entities
{
    public class TechnicianProfile : BaseEntity
    {
        public TechnicianProfile()
        {
            
        }
        public  string UserId { get;  set; }            
        public string Bio { get;set; } = string.Empty;
        public int ExperienceYears { get; set; }
        public double OverallRating { get; set; }      
        public int TotalReviews { get;  set; }
        public int CompletedJobs { get;  set; }  
        public int CancelledJobs { get; set; }
        public int AvgResponseTime { get;  set; }    

        public bool IsApproved { get;  set; } = false;  
        public bool IsFeatured { get; set; } = false;  // ????????????????????????????????
        public string? StripeAccountId { get; set; }
        public bool StripeOnboardingDone { get; set; } = false;

        public SubscriptionTier SubscriptionTier { get;  set; } = SubscriptionTier.Free;
        public DateTime? SubscriptionExpiresAt { get;  set; }
        public DateTime? ApprovedAt { get;  set; }



        public string? EmbeddingVectorJson { get; set; }
        public DateTime? EmbeddingUpdatedAt { get; set; }
        public string? ReviewSummary { get; set; }
        public double? SentimentScore { get; set; }
        public string? TopStrengths { get; set; }
        public string? CommonComplaints { get; set; }
        public DateTime? SummaryUpdatedAt { get; set; }

        [NotMapped]
        public float[]? EmbeddingVector
        {
            get => EmbeddingVectorJson == null
                ? null
                : JsonSerializer.Deserialize<float[]>(EmbeddingVectorJson);
            set => EmbeddingVectorJson = value == null
                ? null
                : JsonSerializer.Serialize(value);
        }




        public User User { get;  set; }
        public ICollection<TechnicianVerification> Verifications { get;  set; }
        public ICollection<TechnicianPortfolio> Portfolio { get; set; }
        public ICollection<TechnicianCategory> Categories { get;  set; }
        public ICollection<Bid> Bids { get; set; }
        public ICollection<FavoriteTechnician> FavoritedByCustomers { get; set; }
    }
}