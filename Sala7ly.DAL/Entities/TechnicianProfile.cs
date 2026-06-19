using System;
using System.Collections.Generic;

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

        public SubscriptionTier SubscriptionTier { get;  set; } = SubscriptionTier.Free;
        public DateTime? SubscriptionExpiresAt { get;  set; }

        public float[] EmbeddingVector { get; set; } = new float [1];
        public DateTime? ApprovedAt { get;  set; }








        public User User { get;  set; }
        public ICollection<TechnicianVerification> Verifications { get;  set; }
        public ICollection<TechnicianPortfolio> Portfolio { get; set; }
        public ICollection<TechnicianCategory> Categories { get;  set; }
        public ICollection<Bid> Bids { get; set; }
        public ICollection<FavoriteTechnician> FavoritedByCustomers { get; set; }
    }
}