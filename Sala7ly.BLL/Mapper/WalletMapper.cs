using Sala7ly.BLL.DTOs.WalletDTOs;
using Sala7ly.DAL.Entities;

namespace Sala7ly.BLL.Mapper
{
    public static class WalletMapper
    {
        public static WalletDto ToDto(Wallet wallet, List<WalletTransactionDto> transactions)
        {
            return new WalletDto
            {
                Balance = wallet.Balance,
                PendingBalance = wallet.PendingBalance,
                TotalEarned = wallet.TotalEarned,
                TotalWithdrawn = wallet.TotalWithdrawn,
                Currency = wallet.Currency.ToString(),
                Transactions = transactions
            };
        }

        public static WalletTransactionDto ToTransactionDto(WalletTransaction t)
        {
            return new WalletTransactionDto
            {
                Id = t.Id,
                Amount = t.Amount,
                BalanceAfter = t.BalanceAfter,
                Type = t.Type.ToString(),
                Description = t.Description,
                Reference = t.Reference,
                CreatedOn = t.CreatedOn ?? DateTime.UtcNow
            };
        }

        public static List<WalletTransactionDto> ToTransactionDtoList(IEnumerable<WalletTransaction> transactions)
            => transactions.Select(ToTransactionDto).ToList();
    }
}
