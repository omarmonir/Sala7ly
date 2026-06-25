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
        public static Bid Create(
           int technicianId,
           int serviceRequestId,
           decimal price,
           string proposalMessage,
           int estimatedDurationMinutes)
        {
            return new Bid
            {
                TechnicianId = technicianId,
                ServiceRequestId = serviceRequestId,
                Price = price,
                ProposalMessage = proposalMessage,
                EstimatedDurationMinutes = estimatedDurationMinutes,
                Status = BidStatus.pending,
                ValidUntil = DateTime.UtcNow.AddHours(24),
                SubmittedAt = DateTime.UtcNow
            };
        }
        public void Accept()
        {
            if (Status != BidStatus.pending)
                throw new InvalidOperationException("Only pending bids can be accepted.");
            Status = BidStatus.accepted;
            RespondedAt = DateTime.UtcNow;
        }

        public void Reject()
        {
            if (Status != BidStatus.pending)
                throw new InvalidOperationException("Only pending bids can be rejected.");
            Status = BidStatus.rejected;
            RespondedAt = DateTime.UtcNow;
        }

        public void Withdraw()
        {
            if (Status != BidStatus.pending)
                throw new InvalidOperationException("Only pending bids can be withdrawn.");
            Status = BidStatus.withdrawn;
            RespondedAt = DateTime.UtcNow;
        }

        public void Expire()
        {
            if (Status == BidStatus.pending)
                Status = BidStatus.expired;
        }
        public void Update(decimal price, string proposalMessage, int estimatedDurationMinutes)
        {
            if (Status != BidStatus.pending)
                throw new InvalidOperationException("يمكن تعديل العروض المعلقة فقط.");

            if (price <= 0)
                throw new InvalidOperationException("السعر يجب أن يكون أكبر من صفر.");

            if (string.IsNullOrWhiteSpace(proposalMessage))
                throw new InvalidOperationException("رسالة العرض مطلوبة.");

            Price = price;
            ProposalMessage = proposalMessage;
            EstimatedDurationMinutes = estimatedDurationMinutes;
        }
    }



}