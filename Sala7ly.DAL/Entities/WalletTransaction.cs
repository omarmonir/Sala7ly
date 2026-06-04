using Sala7ly.DAL.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sala7ly.DAL.Entities
{
    public class WalletTransaction
    {

        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid WalletId { get; set; }

        public Guid? EscrowId { get; set; }

        public Guid? PromotionId { get; set; }

        public decimal Amount { get; set; }

        public decimal BalanceAfter { get; set; }

        public WalletTransactionType Type { get; set; }

        public string Description { get; set; }

        public string Reference { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // NP
        public Wallet Wallet { get; set; }

        // public EscrowTransaction EscrowTransaction { get; set; }

        // public Promotion Promotion { get; set; }


    }
}
