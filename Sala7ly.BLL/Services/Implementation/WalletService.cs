using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Stripe;
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
            var wallet = await GetOrCreateWalletAsync(userId);
            var transactions = await _transactionRepo.GetByWalletIdAsync(wallet.Id, page, pageSize);
            return WalletMapper.ToDto(wallet, WalletMapper.ToTransactionDtoList(transactions));
        }

        public async Task<TopUpResultDto> TopUpAsync(string userId, TopUpDto dto)
        {
            if (dto.Amount <= 0)
                throw new Exception("Top-up amount must be greater than zero.");

            if (string.IsNullOrWhiteSpace(dto.PaymentMethodId))
                throw new Exception("Payment method is required.");

            await GetOrCreateWalletAsync(userId);

            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(dto.Amount * 100),
                Currency = "egp",
                PaymentMethod = dto.PaymentMethodId,
                ConfirmationMethod = "automatic",
                Confirm = true,
                PaymentMethodTypes = new List<string> { "card" },
                Metadata = new Dictionary<string, string>
                {
                    { "payment_type", "wallet_topup" },
                    { "user_id", userId }
                }
            };

            var service = new PaymentIntentService();
            var intent = await service.CreateAsync(options);

            return new TopUpResultDto
            {
                ClientSecret = intent.ClientSecret,
                PaymentIntentId = intent.Id,
                Amount = dto.Amount
            };
        }

        public async Task WithdrawAsync(string userId, WithdrawDto dto)
        {
            if (dto.Amount <= 0)
                throw new Exception("Withdrawal amount must be greater than zero.");

            if (string.IsNullOrWhiteSpace(dto.BankAccount))
                throw new Exception("Bank account is required.");

            var wallet = await GetOrCreateWalletAsync(userId);
            if (wallet.Balance < dto.Amount)
                throw new Exception("Insufficient balance.");

            wallet.Balance -= dto.Amount;
            wallet.TotalWithdrawn += dto.Amount;

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
            await _walletRepo.SaveChangesAsync();

            await _notificationService.NotifyUserAsync(
                userId,
                NotificationType.payment,
                "تم طلب سحب المحفظة",
                $"تم طلب سحب مبلغ {dto.Amount} {wallet.Currency} إلى الحساب {dto.BankAccount}.",
                null,
                "/wallet"
            );
        }

        public async Task CreditAsync(string userId, decimal amount, string description, WalletTransactionType type, int? escrowId = null)
        {
            if (amount <= 0)
                throw new Exception("Credit amount must be greater than zero.");

            var wallet = await GetOrCreateWalletAsync(userId);
            wallet.Balance += amount;

            if (type == WalletTransactionType.payout)
                wallet.TotalEarned += amount;

            var tx = new WalletTransaction
            {
                WalletId = wallet.Id,
                EscrowTransactionId = escrowId,
                Amount = amount,
                BalanceAfter = wallet.Balance,
                Type = type,
                Description = description
            };
            tx.MarkCreated(userId);

            await _transactionRepo.AddAsync(tx);
            await _walletRepo.SaveChangesAsync();
        }

        public async Task DebitAsync(string userId, decimal amount, string description, WalletTransactionType type, int? escrowId = null)
        {
            if (amount <= 0)
                throw new Exception("Debit amount must be greater than zero.");

            var wallet = await GetOrCreateWalletAsync(userId);
            if (wallet.Balance < amount)
                throw new Exception("Insufficient balance.");

            wallet.Balance -= amount;

            var tx = new WalletTransaction
            {
                WalletId = wallet.Id,
                EscrowTransactionId = escrowId,
                Amount = -amount,
                BalanceAfter = wallet.Balance,
                Type = type,
                Description = description
            };
            tx.MarkCreated(userId);

            await _transactionRepo.AddAsync(tx);
            await _walletRepo.SaveChangesAsync();
        }

        public async Task TransferAsync(string fromUserId, string toUserId, decimal amount, string description)
        {
            if (string.IsNullOrWhiteSpace(toUserId))
                throw new Exception("Recipient user ID is required.");

            if (fromUserId == toUserId)
                throw new Exception("Cannot transfer to the same wallet.");

            if (amount <= 0)
                throw new Exception("Transfer amount must be greater than zero.");

            var senderWallet = await GetOrCreateWalletAsync(fromUserId);
            if (senderWallet.Balance < amount)
                throw new Exception("Insufficient balance.");

            var recipientWallet = await GetOrCreateWalletAsync(toUserId);

            senderWallet.Balance -= amount;
            recipientWallet.Balance += amount;

            var debitTx = new WalletTransaction
            {
                WalletId = senderWallet.Id,
                Amount = -amount,
                BalanceAfter = senderWallet.Balance,
                Type = WalletTransactionType.transfer,
                Description = description,
                Reference = toUserId
            };
            debitTx.MarkCreated(fromUserId);

            var creditTx = new WalletTransaction
            {
                WalletId = recipientWallet.Id,
                Amount = amount,
                BalanceAfter = recipientWallet.Balance,
                Type = WalletTransactionType.transfer,
                Description = description,
                Reference = fromUserId
            };
            creditTx.MarkCreated(fromUserId);

            await _transactionRepo.AddAsync(debitTx);
            await _transactionRepo.AddAsync(creditTx);
            await _walletRepo.SaveChangesAsync();

            await _notificationService.NotifyUserAsync(
                toUserId,
                NotificationType.payment,
                "تم استلام تحويل",
                $"استلمت مبلغ {amount} {recipientWallet.Currency} من محفظة المستخدم.",
                fromUserId,
                "/wallet"
            );
        }

        public async Task HandleStripePaymentIntentSucceededAsync(PaymentIntent intent)
        {
            if (intent == null || intent.Metadata == null)
                return;

            if (!intent.Metadata.TryGetValue("payment_type", out var paymentType) || paymentType != "wallet_topup")
                return;

            if (!intent.Metadata.TryGetValue("user_id", out var userId))
                return;

            var wallet = await GetOrCreateWalletAsync(userId);
            var reference = intent.Id;
            if (await _transactionRepo.ExistsByReferenceAsync(reference, WalletTransactionType.deposit, wallet.Id))
                return;

            var amount = intent.AmountReceived / 100m;
            wallet.Balance += amount;

            var tx = new WalletTransaction
            {
                WalletId = wallet.Id,
                Amount = amount,
                BalanceAfter = wallet.Balance,
                Type = WalletTransactionType.deposit,
                Description = "شحن المحفظة",
                Reference = reference
            };
            tx.MarkCreated(userId);

            await _transactionRepo.AddAsync(tx);
            await _walletRepo.SaveChangesAsync();

            await _notificationService.NotifyUserAsync(
                userId,
                NotificationType.payment,
                "تم شحن المحفظة",
                $"تم شحن محفظتك بمبلغ {amount} {wallet.Currency}.",
                null,
                "/wallet"
            );
        }

        public async Task HandleStripeChargeRefundedAsync(Charge charge)
        {
            if (charge == null || string.IsNullOrWhiteSpace(charge.PaymentIntentId))
                return;

            var reference = charge.PaymentIntentId;
            var depositTx = await _transactionRepo.GetByReferenceAndTypeAsync(reference, WalletTransactionType.deposit);
            if (depositTx == null)
                return;

            var wallet = await _walletRepo.GetByIdAsync(depositTx.WalletId);
            if (wallet == null)
                return;

            var refundedAmount = charge.AmountRefunded / 100m;
            var existingRefunds = await _transactionRepo.GetByReferenceAsync(reference, WalletTransactionType.refund);
            var refundedSoFar = existingRefunds.Sum(t => Math.Abs(t.Amount));
            var amountToDebit = refundedAmount - refundedSoFar;

            if (amountToDebit <= 0)
                return;

            if (wallet.Balance < amountToDebit)
                throw new Exception("Cannot process refund because wallet balance is insufficient.");

            wallet.Balance -= amountToDebit;

            var tx = new WalletTransaction
            {
                WalletId = wallet.Id,
                Amount = -amountToDebit,
                BalanceAfter = wallet.Balance,
                Type = WalletTransactionType.refund,
                Description = "استرداد شحن المحفظة",
                Reference = reference
            };
            tx.MarkCreated(wallet.UserId);

            await _transactionRepo.AddAsync(tx);
            await _walletRepo.SaveChangesAsync();
        }

        private async Task<Wallet> GetOrCreateWalletAsync(string userId)
        {
            var wallet = await _walletRepo.GetByUserIdAsync(userId);
            if (wallet != null)
                return wallet;

            wallet = new Wallet
            {
                UserId = userId,
                Balance = 0,
                PendingBalance = 0,
                TotalEarned = 0,
                TotalWithdrawn = 0,
                Currency = Currencies.EGP
            };

            await _walletRepo.AddAsync(wallet);
            await _walletRepo.SaveChangesAsync();
            return wallet;
        }
    }
}
