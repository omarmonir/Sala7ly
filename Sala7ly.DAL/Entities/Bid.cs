using Sala7ly.DAL.Enums;

namespace Sala7ly.DAL.Entities
{
    public class Bid : BaseEntity
    {
        public Bid() { }

        public int TechnicianId { get; private set; }
        public int ServiceRequestId { get; private set; }
        public decimal Price { get; private set; }
        public string ProposalMessage { get; private set; }
        public int EstimatedDurationMinutes { get; private set; }
        public BidStatus Status { get; private set; } = BidStatus.pending;
        public DateTime ValidUntil { get; private set; }  // when the bid expires if not accepted

        public DateTime SubmittedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? RespondedAt { get; private set; }  // when customer accepted/rejected the bid
        public bool IsAccepted { get; private set; } = false;

        // navigation
        public TechnicianProfile Technician { get; private set; }
        public ServiceRequest ServiceRequest { get; private set; }
    }



}