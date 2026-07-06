using Sala7ly.BLL.DTOs.WalletDTOs;
using Sala7ly.DAL.Enums;
using Stripe;

namespace Sala7ly.BLL.Services.Abstraction
{
    public interface IWalletService
    {
        Task<WalletDto> GetWalletAsync(string userId, int page, int pageSize);
        Task<TopUpResultDto> TopUpAsync(string userId, TopUpDto dto);
        Task WithdrawAsync(string userId, WithdrawDto dto);
        Task TransferAsync(string fromUserId, string toUserId, decimal amount, string description);
        Task CreditAsync(string userId, decimal amount, string description, WalletTransactionType type, int? escrowId = null);
        Task DebitAsync(string userId, decimal amount, string description, WalletTransactionType type, int? escrowId = null);
        Task HandleStripePaymentIntentSucceededAsync(PaymentIntent intent);
        Task HandleStripeChargeRefundedAsync(Charge charge);
        Task<bool> ConfirmTopUpAsync(string sessionId, string currentUserId);
    }
}
