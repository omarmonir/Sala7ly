using System;

namespace Sala7ly.DAL.Entities
{
    public class TechnicianVerification
    {
        public int Id { get; set; }

        public int TechnicianId { get; set; }          
        public int? ReviewedByAdminId { get; set; }    

        public VerificationDocType DocType { get; set; }
        public string DocumentUrl { get; set; }
        public VerificationStatus Status { get; set; } = VerificationStatus.Pending;
        public string RejectionReason { get; set; }    
        public DateTime SubmittedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }












        // navigation
        public TechnicianProfile Technician { get; set; }
    }
}