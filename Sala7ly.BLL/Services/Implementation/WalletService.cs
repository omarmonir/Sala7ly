using Sala7ly.BLL.DTOs.WalletDTOs;
using Sala7ly.BLL.Mapper;
using Sala7ly.BLL.Services.Abstraction;
using Sala7ly.DAL.Entities;
using Sala7ly.DAL.Enums;
using Sala7ly.DAL.Repositories.Abstraction;

namespace Sala7ly.BLL.Services.Implementation
{
    public class WalletService : IWalletService
    {
        private readonly IWalletRepository _walletRepo;
        private readonly IWalletTransactionRepository _transactionRepo;
        private readonly INotificationService _notificationService;

        public WalletService(
            IWalletRepository walletRepo,
            IWalletTransactionRepository transactionRepo,
            INotificationService notificationService)
        {
            _walletRepo = walletRepo;
            _transactionRepo = transactionRepo;
            _notificationService = notificationService;
        }

        public async Task<WalletDto> GetWalletAsync(string userId, int page, int pageSize)
        {
            var wallet = await _walletRepo.GetByUserIdAsync(userId);
            if (wallet == null)
                throw new Exception("Wallet not found.");

            var transactions = await _transactionRepo
                .GetByWalletIdAsync(wallet.Id, page, pageSize);

            return WalletMapper.ToDto(wallet, WalletMapper.ToTransactionDtoList(transactions));
        }

        public async Task CreditAsync(string userId, decimal amount, string description, int? escrowId = null)
        {
            var wallet = await _walletRepo.GetByUserIdAsync(userId);
            if (wallet == null) throw new Exception("Wallet not found.");

            wallet.Balance += amount;
            wallet.TotalEarned += amount;

            await _walletRepo.SaveChangesAsync();

            // Log transaction
            var tx = new WalletTransaction
            {
                WalletId = wallet.Id,
                EscrowTransactionId = escrowId,
                Amount = amount,
                BalanceAfter = wallet.Balance,
                Type = WalletTransactionType.credit,
                Description = description,
            };
            tx.MarkCreated(userId);

            await _transactionRepo.AddAsync(tx);
            await _transactionRepo.SaveChangesAsync();
        }

        public async Task DebitAsync(string userId, decimal amount, string description, int? escrowId = null)
        {
            var wallet = await _walletRepo.GetByUserIdAsync(userId);
            if (wallet == null) throw new Exception("Wallet not found.");
            if (wallet.Balance < amount) throw new Exception("Insufficient balance.");

            wallet.Balance -= amount;

            await _walletRepo.SaveChangesAsync();

            var tx = new WalletTransaction
            {
                WalletId = wallet.Id,
                EscrowTransactionId = escrowId,
                Amount = -amount,
                BalanceAfter = wallet.Balance,
                Type = WalletTransactionType.debit,
                Description = description,
            };
            tx.MarkCreated(userId);

            await _transactionRepo.AddAsync(tx);
            await _transactionRepo.SaveChangesAsync();
        }

        public async Task TopUpAsync(string userId, TopUpDto dto)
        {
            // Stripe handles actual charge — this records it after webhook confirms
            var wallet = await _walletRepo.GetByUserIdAsync(userId);
            if (wallet == null) throw new Exception("Wallet not found.");

            wallet.Balance += dto.Amount;
            await _walletRepo.SaveChangesAsync();

            var tx = new WalletTransaction
            {
                WalletId = wallet.Id,
                Amount = dto.Amount,
                BalanceAfter = wallet.Balance,
                Type = WalletTransactionType.promotion,
                Description = "شحن المحفظة"
            };
            tx.MarkCreated(userId);

            await _transactionRepo.AddAsync(tx);
            await _transactionRepo.SaveChangesAsync();

          
        }

        public async Task WithdrawAsync(string userId, WithdrawDto dto)
        {
            var wallet = await _walletRepo.GetByUserIdAsync(userId);
            if (wallet == null) throw new Exception("Wallet not found.");
            if (wallet.Balance < dto.Amount) throw new Exception("Insufficient balance.");

            wallet.Balance -= dto.Amount;
            wallet.TotalWithdrawn += dto.Amount;
            await _walletRepo.SaveChangesAsync();

            var tx = new WalletTransaction
            {
                WalletId = wallet.Id,
                Amount = -dto.Amount,
                BalanceAfter = wallet.Balance,
                Type = WalletTransactionType.withdrawal,
                Description = $"سحب إلى {dto.BankAccount}",
                Reference = dto.BankAccount
            };
            tx.MarkCreated(userId);

            await _transactionRepo.AddAsync(tx);
            await _transactionRepo.SaveChangesAsync();

        }
    }

}
