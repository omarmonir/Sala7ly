using System;

namespace Sala7ly.DAL.Entities
{
    public class Dispute : BaseEntity
    {
        public int EscrowTransactionId { get; private set; }
        public int ServiceRequestId { get; private set; }
        public string InitiatedByUserId { get; private set; }  // customer or technician
        public string? ResolvedByAdminId { get; private set; }  // nullable until resolved

        public DisputeStatus Status { get; private set; } = DisputeStatus.Open;
        public string Reason { get; private set; } = string.Empty;  // why dispute was opened
        public string Resolution { get; private set; } = string.Empty;  // how it was resolved
        public decimal RefundAmount { get; private set; }  // if applicable

        public DateTime? ResolvedAt { get; private set; }

        // navigation
        public EscrowTransaction EscrowTransaction { get; private set; }
        public ServiceRequest ServiceRequest { get; private set; }

        public static Dispute Create(
            int escrowTransactionId,
            int serviceRequestId,
            string initiatedByUserId,
            string reason)
        {
            if (string.IsNullOrWhiteSpace(initiatedByUserId))
                throw new ArgumentException("Initiator user ID is required.", nameof(initiatedByUserId));
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Reason is required.", nameof(reason));

            return new Dispute
            {
                EscrowTransactionId = escrowTransactionId,
                ServiceRequestId = serviceRequestId,
                InitiatedByUserId = initiatedByUserId,
                Reason = reason.Trim(),
                Status = DisputeStatus.Open,
                CreatedOn = DateTime.UtcNow
            };
        }

        public void Resolve(string resolvedByAdminId, string resolution, decimal refundAmount)
        {
            if (Status != DisputeStatus.Open)
                throw new InvalidOperationException("Only open disputes can be resolved.");
            if (string.IsNullOrWhiteSpace(resolvedByAdminId))
                throw new ArgumentException("Admin ID is required.", nameof(resolvedByAdminId));
            if (string.IsNullOrWhiteSpace(resolution))
                throw new ArgumentException("Resolution text is required.", nameof(resolution));
            if (refundAmount < 0)
                throw new ArgumentOutOfRangeException(nameof(refundAmount), "Refund amount cannot be negative.");

            ResolvedByAdminId = resolvedByAdminId;
            Resolution = resolution.Trim();
            RefundAmount = refundAmount;
            ResolvedAt = DateTime.UtcNow;
            Status = DisputeStatus.Resolved;
        }

        public void Close()
        {
            if (Status != DisputeStatus.Resolved)
                throw new InvalidOperationException("Only resolved disputes can be closed.");

            Status = DisputeStatus.Closed;
        }
    }
}
