using Sala7ly.DAL.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sala7ly.DAL.Entities
{
    public class WalletTransaction : BaseEntity
    {


        public int WalletId { get; set; }

        public int? EscrowTransactionId { get; set; }

        public decimal Amount { get; set; }

        public decimal BalanceAfter { get; set; }

        public WalletTransactionType Type { get; set; }

        public string Description { get; set; }

        public string Reference { get; set; }


        // NP
        public Wallet Wallet { get; set; }

        public EscrowTransaction EscrowTransaction { get; set; }



    }
}
