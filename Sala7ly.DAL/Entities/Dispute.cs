using System;

namespace Sala7ly.DAL.Entities
{
    public class Dispute : BaseEntity
    {
        public int EscrowTransactionId { get; private set; }
        public int ServiceRequestId { get; private set; }
        public int InitiatedByUserId { get; private set; }  // customer or technician
        public int? ResolvedByAdminId { get; private set; }  // nullable until resolved

        public DisputeStatus Status { get; private set; } = DisputeStatus.Open;
        public string Reason { get; private set; }  // why dispute was opened
        public string Resolution { get; private set; }  // how it was resolved
        public decimal RefundAmount { get; private set; }  // if applicable

        public DateTime? ResolvedAt { get; private set; }

        // navigation
        public EscrowTransaction EscrowTransaction { get; private set; }
        public ServiceRequest ServiceRequest { get; private set; }
    }
}
