using System;

namespace Sala7ly.BLL.DTOs.VerificationDTOs
{
    public class VerificationDetailsDto
    {
        public int Id { get; set; }
        public int TechnicianId { get; set; }
        public string TechnicianName { get; set; }
        //public string DocType { get; set; }
        public string DocumentUrlFront { get; set; }
        public string DocumentUrlBack { get; set; }
        public string Status { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime SubmittedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }
}