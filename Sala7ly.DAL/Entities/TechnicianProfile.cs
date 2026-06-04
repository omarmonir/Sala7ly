using System;
using System.Collections.Generic;

namespace Sala7ly.DAL.Entities
{
    public class TechnicianProfile : BaseEntity
    {
        public TechnicianProfile()
        {
            
        }
        public  string UserId { get; private set; }            

        public string Bio { get; private set; }
        public int ExperienceYears { get; private set; }
        public double OverallRating { get; private set; }      
        public int TotalReviews { get; private set; }
        public int CompletedJobs { get; private set; }  
        public int CancelledJobs { get; private set; }
        public string AvgResponseTime { get; private set; }    

        public bool IsApproved { get; private set; } = false;  
        public bool IsFeatured { get; private set; } = false;  // ????????????????????????????????

        public SubscriptionTier SubscriptionTier { get; private set; } = SubscriptionTier.Free;
        public DateTime? SubscriptionExpiresAt { get; private set; }

        public float[] EmbeddingVector { get; private set; }  // AI semantic matching
        public DateTime? ApprovedAt { get; private set; }








        public User User { get; private set; }
        public ICollection<TechnicianVerification> Verifications { get; private set; }
        public ICollection<TechnicianPortfolio> Portfolio { get; private set; }
        public ICollection<TechnicianCategory> Categories { get; private set; }
        public ICollection<Bid> Bids { get; private set; }
        public ICollection<FavoriteTechnician> FavoritedByCustomers { get; private set; }
    }
}