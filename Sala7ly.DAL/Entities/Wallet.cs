using Sala7ly.DAL.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Sala7ly.DAL.Entities
{
    public class Wallet
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; }

        public decimal Balance { get; set; }

        public decimal PendingBalance { get; set; }

        public decimal TotalEarned { get; set; }

        public decimal TotalWithdrawn { get; set; }

        public Currencies Currency { get; set; } = Currencies.EGP;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


        // NP

        //public User User { get; set; }

        public ICollection<WalletTransaction> Transactions { get; set; } = new List<WalletTransaction>();


    }
}