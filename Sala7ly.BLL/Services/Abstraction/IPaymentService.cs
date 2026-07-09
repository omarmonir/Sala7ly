using Sala7ly.BLL.DTOs.PaymentDTOs;

namespace Sala7ly.BLL.Services.Abstraction
{
    public interface IPaymentService
    {
        Task<CreatePaymentResultDto> CreateEscrowAsync(string customerUserId, CreatePaymentDto dto);
        Task ReleasePaymentAsync(int requestId, string customerUserId);
        Task RefundPaymentAsync(int requestId, string customerUserId);
        Task<EscrowDto?> GetEscrowByRequestAsync(int requestId);

        Task HandlePaymentHeldAsync(string paymentIntentId, string chargeId);
        Task HandlePaymentReleasedAsync(string paymentIntentId);
        Task HandlePaymentRefundedAsync(string paymentIntentId);
    }
}
