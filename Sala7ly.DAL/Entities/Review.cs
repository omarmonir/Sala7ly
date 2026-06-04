using System;
using System.Collections.Generic;
using System.Text;

namespace Sala7ly.DAL.Entities
{
    public class Review : BaseEntity
    {

        public int RequestId { get; set; }

        public string ReviewerID { get; set; }

        public string RevieweeID { get; set; }

        public int QualityScore { get; set; }

        public int PunctualityScore { get; set; }

        public int CommunicationScore { get; set; }

        public int ValueScore { get; set; }

        public float OverallScore { get; set; }

        public string? Comment { get; set; }

        public string? TechnicianReply { get; set; }

        public bool IsModerated { get; set; }

        public string? ModerationNote { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ModeratedAt { get; set; }

        // NP

        public ServiceRequest ServiceRequest { get; set; }

        public User Reviewer { get; set; }

        public User Reviewee { get; set; }


    }
}
