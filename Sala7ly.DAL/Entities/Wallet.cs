using Sala7ly.DAL.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Sala7ly.DAL.Entities
{
    public class Wallet : BaseEntity
    {

        public string UserId { get; set; }

        public decimal Balance { get; set; }

        public decimal PendingBalance { get; set; }

        public decimal TotalEarned { get; set; }

        public decimal TotalWithdrawn { get; set; }

        public Currencies Currency { get; set; } = Currencies.EGP;



        // NP

        public User User { get; set; }

        public ICollection<WalletTransaction> Transactions { get; set; } = new List<WalletTransaction>();


    }
}