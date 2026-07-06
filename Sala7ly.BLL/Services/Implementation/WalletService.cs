using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
        private readonly IConfiguration _config;
        private readonly ILogger<WalletService> _logger;

        public WalletService(
            IWalletRepository walletRepo,
            IWalletTransactionRepository transactionRepo,
            INotificationService notificationService,
            IConfiguration config,
            ILogger<WalletService> logger)
        {
            _walletRepo = walletRepo;
            _transactionRepo = transactionRepo;
            _notificationService = notificationService;
            _config = config;
            _logger = logger;
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

            var frontendBaseUrl = _config["FrontendUrl"] ?? _config["App:FrontendUrl"] ?? "https://sala7ly.runasp.net";
            var successUrl = $"{frontendBaseUrl}/customer/wallet?payment=success&session_id={{CHECKOUT_SESSION_ID}}";
            var cancelUrl = $"{frontendBaseUrl}/customer/wallet?payment=cancel";

            var options = new Stripe.Checkout.SessionCreateOptions
            {
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<Stripe.Checkout.SessionLineItemOptions>
                {
                    new()
                    {
                        PriceData = new Stripe.Checkout.SessionLineItemPriceDataOptions
                        {
                            Currency = "egp",
                            UnitAmount = (long)(dto.Amount * 100),
                            ProductData = new Stripe.Checkout.SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "شحن محفظة Sala7ly"
                            }
                        },
                        Quantity = 1
                    }
                },
                Metadata = new Dictionary<string, string>
                {
                    { "payment_type", "wallet_topup" },
                    { "user_id", userId }
                },
                PaymentIntentData = new Stripe.Checkout.SessionPaymentIntentDataOptions
                {
                    Metadata = new Dictionary<string, string>
                    {
                        { "payment_type", "wallet_topup" },
                        { "user_id", userId }
                    }
                }
            };

            var service = new Stripe.Checkout.SessionService();
            var session = await service.CreateAsync(options);

            _logger?.LogInformation("Created Stripe checkout session {SessionId} for user {UserId} amount {Amount}", session?.Id, userId, dto.Amount);

            return new TopUpResultDto
            {
                ClientSecret = session.Id,
                PaymentIntentId = session.PaymentIntentId,
                CheckoutUrl = session.Url,
                SessionId = session.Id,
                Amount = dto.Amount
            };
        }

        public async Task<bool> ConfirmTopUpAsync(string sessionId, string currentUserId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                return false;
            try
            {
                var sessionService = new Stripe.Checkout.SessionService();
                var session = await sessionService.GetAsync(sessionId);
                if (session == null)
                {
                    _logger?.LogWarning("ConfirmTopUp: session not found {SessionId}", sessionId);
                    return false;
                }

                if (string.IsNullOrWhiteSpace(session.PaymentIntentId))
                {
                    _logger?.LogWarning("ConfirmTopUp: session {SessionId} has no PaymentIntentId", sessionId);
                    return false;
                }

                var paymentIntentService = new Stripe.PaymentIntentService();
                var paymentIntent = await paymentIntentService.GetAsync(session.PaymentIntentId);
                if (paymentIntent == null)
                {
                    _logger?.LogWarning("ConfirmTopUp: payment intent not found {PaymentIntentId}", session.PaymentIntentId);
                    return false;
                }

                _logger?.LogInformation("ConfirmTopUp: session {SessionId} paymentIntent {PaymentIntentId} status {Status} amount_received {AmountReceived}", sessionId, paymentIntent.Id, paymentIntent.Status, paymentIntent.AmountReceived);

                // Validate that the session/payment belongs to the currently authenticated user
                if (paymentIntent.Metadata != null && paymentIntent.Metadata.TryGetValue("user_id", out var userId))
                {
                    if (!string.Equals(userId, currentUserId, StringComparison.OrdinalIgnoreCase))
                    {
                        _logger?.LogWarning("ConfirmTopUp: payment intent user_id {UserId} does not match current user {CurrentUser}", userId, currentUserId);
                        return false;
                    }
                }

                // If payment succeeded, process the credit logic (idempotent inside)
                if (string.Equals(paymentIntent.Status, "succeeded", StringComparison.OrdinalIgnoreCase) || string.Equals(paymentIntent.Status, "requires_capture", StringComparison.OrdinalIgnoreCase) || paymentIntent.AmountReceived > 0)
                {
                    await HandleStripePaymentIntentSucceededAsync(paymentIntent);
                    _logger?.LogInformation("ConfirmTopUp: processed top-up for session {SessionId}", sessionId);
                    return true;
                }

                _logger?.LogWarning("ConfirmTopUp: payment intent {PaymentIntentId} not paid yet. Status={Status} AmountReceived={AmountReceived}", paymentIntent.Id, paymentIntent.Status, paymentIntent.AmountReceived);
                return false;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "ConfirmTopUp: exception while confirming session {SessionId}", sessionId);
                return false;
            }
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
            if (intent == null)
            {
                _logger?.LogWarning("HandleStripePaymentIntentSucceededAsync called with null intent");
                return;
            }

            _logger?.LogInformation("HandleStripePaymentIntentSucceededAsync: intent {Id} status {Status} amount_received {AmountReceived}", intent.Id, intent.Status, intent.AmountReceived);

            if (intent.Metadata == null)
            {
                _logger?.LogWarning("PaymentIntent {Id} has no metadata", intent.Id);
                return;
            }

            if (!intent.Metadata.TryGetValue("payment_type", out var paymentType) || paymentType != "wallet_topup")
            {
                _logger?.LogInformation("PaymentIntent {Id} payment_type {PaymentType} ignored", intent.Id, paymentType);
                return;
            }

            if (!intent.Metadata.TryGetValue("user_id", out var userId))
            {
                _logger?.LogWarning("PaymentIntent {Id} missing user_id metadata", intent.Id);
                return;
            }

            var wallet = await GetOrCreateWalletAsync(userId);
            var reference = intent.Id;
            if (await _transactionRepo.ExistsByReferenceAsync(reference, WalletTransactionType.deposit, wallet.Id))
            {
                _logger?.LogInformation("PaymentIntent {Id} already processed for wallet {WalletId}", intent.Id, wallet.Id);
                return;
            }

            var amount = intent.AmountReceived / 100m;
            if (amount <= 0)
            {
                _logger?.LogWarning("PaymentIntent {Id} amount received is zero", intent.Id);
                return;
            }

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

            _logger?.LogInformation("Credited wallet {WalletId} user {UserId} amount {Amount}", wallet.Id, userId, amount);

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
