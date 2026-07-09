using System;
using System.Collections.Generic;
using System.Text;

namespace Sala7ly.BLL.DTOs.WalletDTOs
{
    public class WalletDto
    {
        public decimal Balance { get; set; }
        public decimal PendingBalance { get; set; }
        public decimal TotalEarned { get; set; }
        public decimal TotalWithdrawn { get; set; }
        public string Currency { get; set; }
        public List<WalletTransactionDto> Transactions { get; set; }
    }
}
