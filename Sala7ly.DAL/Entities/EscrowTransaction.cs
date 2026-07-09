using System;
using System.Collections.Generic;
using Sala7ly.DAL.Enums;

namespace Sala7ly.DAL.Entities
{
    public class EscrowTransaction : BaseEntity
    {
        public int ServiceRequestId { get; private set; }
        public int CustomerId { get; private set; }
        public int TechnicianId { get; private set; }
        public int? DisputeId { get; private set; }  // nullable until disputed

        public decimal Amount { get; private set; }  // total charged to customer
        public decimal PlatformFee { get; private set; }  // Sala7ly commission
        public decimal TechnicianPayout { get; private set; }  // amount - platform_fee

        public EscrowStatus Status { get; private set; } = EscrowStatus.PendingDeposit;
        public string ProviderRef { get; private set; }  // external transaction ID
        public string? StripeChargeId { get; private set; }
        public string ProviderReceiptUrl { get; private set; }  // nullable

        public DateTime? DepositedAt { get; private set; }
        public DateTime? ReleasedAt { get; private set; }
        public DateTime? RefundedAt { get; private set; }

        // navigation
        public ServiceRequest ServiceRequest { get; private set; }
        public CustomerProfile Customer { get; private set; }
        public TechnicianProfile Technician { get; private set; }
        public Dispute Dispute { get; private set; }
        public ICollection<WalletTransaction> WalletTransactions { get; private set; }
        public static EscrowTransaction Create(
            int serviceRequestId,
            int customerId,
            int technicianId,
            decimal amount,
            decimal platformFeePercent,
            string stripePaymentIntentId)
        {
            var fee = Math.Round(amount * platformFeePercent, 2);
            var payout = amount - fee;

            return new EscrowTransaction
            {
                ServiceRequestId = serviceRequestId,
                CustomerId = customerId,
                TechnicianId = technicianId,
                Amount = amount,
                PlatformFee = fee,
                TechnicianPayout = payout,
                Status = EscrowStatus.PendingDeposit,
                ProviderRef = stripePaymentIntentId
            };
        }

        // ── Domain Methods ────────────────────────────────────
        public void MarkDeposited(string chargeId)
        {
            Status = EscrowStatus.Held;
            StripeChargeId = chargeId;
            DepositedAt = DateTime.UtcNow;
        }

        public void MarkReleased()
        {
            Status = EscrowStatus.Released;
            ReleasedAt = DateTime.UtcNow;
        }

        public void MarkRefunded()
        {
            Status = EscrowStatus.Refunded;
            RefundedAt = DateTime.UtcNow;
        }

        public void MarkDisputed(int disputeId)
        {
            Status = EscrowStatus.Disputed;
            DisputeId = disputeId;
        }

        public void SetReceiptUrl(string url)
        {
            ProviderReceiptUrl = url;
        }
    }
}
