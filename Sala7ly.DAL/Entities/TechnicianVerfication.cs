using System;

namespace Sala7ly.DAL.Entities
{
    public class TechnicianVerification : BaseEntity
    {
        public int TechnicianId { get; set; }
        public string? ReviewedByAdminId { get; set; }   // FK → User (string id), nullable

        //public VerificationDocType DocType { get; set; }
        public string DocumentUrlFront { get; set; }   
        public string DocumentUrlBack { get; set; }
        public string IdNumber { get; set; }
        public List<string> DegreeCertificateUrls { get; set; } = new List<string>();
        public VerificationStatus Status { get; set; } = VerificationStatus.Pending;
        public string? RejectionReason { get; set; }     // nullable
        public DateTime SubmittedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }

        // navigation
        public TechnicianProfile Technician { get; set; }
    }
}