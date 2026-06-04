using System;

namespace Sala7ly.DAL.Entities
{
    public class TechnicianVerification : BaseEntity
    {
        public int TechnicianId { get; private set; }          
        public int? ReviewedByAdminId { get; private set; }    

        public VerificationDocType DocType { get; private set; }
        public string DocumentUrl { get; private set; }
        public VerificationStatus Status { get; private set; } = VerificationStatus.Pending;
        public string RejectionReason { get; private set; }    
        public DateTime SubmittedAt { get; private set; }
        public DateTime? ReviewedAt { get; private set; }












        // navigation
        public TechnicianProfile Technician { get; private set; }
    }
}