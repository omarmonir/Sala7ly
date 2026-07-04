using Microsoft.Extensions.Configuration;
using Sala7ly.BLL.DTOs.PaymentDTOs;
using Sala7ly.BLL.Mapper;
using Sala7ly.BLL.Services.Abstraction;
using Sala7ly.DAL.Entities;
using Sala7ly.DAL.Enums;
using Sala7ly.DAL.Repositories.Abstraction;
using Stripe;

namespace Sala7ly.BLL.Services.Implementation
{
    public class PaymentService : IPaymentService
    {
        private readonly IEscrowRepository _escrowRepo;
        private readonly IWalletService _walletService;
        private readonly IServiceRequestRepository _requestRepo;
        private readonly ICustomerRepository _customerRepo;
        private readonly ITechnicianProfileRepository _technicianRepo;
        private readonly INotificationService _notificationService;
        private readonly IConfiguration _config;

        private const decimal PlatformFeePercent = 0.10m; 

        public PaymentService(
            IEscrowRepository escrowRepo,
            IWalletService walletService,
            IServiceRequestRepository requestRepo,
            ICustomerRepository customerRepo,
            ITechnicianProfileRepository technicianRepo,
            INotificationService notificationService,
            IConfiguration config)
        {
            _escrowRepo = escrowRepo;
            _walletService = walletService;
            _requestRepo = requestRepo;
            _customerRepo = customerRepo;
            _technicianRepo = technicianRepo;
            _notificationService = notificationService;
            _config = config;
        }

        public async Task<CreatePaymentResultDto> CreateEscrowAsync(string customerUserId, CreatePaymentDto dto)
        {
            var request = await _requestRepo.GetByIdWithPartiesAsync(dto.RequestId);
            if (request == null)
                throw new Exception("Request not found.");
            if (request.Status != Status.assigned)
                throw new Exception("Request must be assigned before payment.");
            if (request.SelectedBid == null)
                throw new Exception("No accepted bid found.");

            var existing = await _escrowRepo.GetByRequestIdAsync(dto.RequestId);
            if (existing != null)
                throw new Exception("Payment already created for this request.");

            var customer = await _customerRepo.GetByUserIdAsync(customerUserId);
            var technician = await _technicianRepo.GetByIdAsync(request.SelectedBid.TechnicianId);
            var amount = request.SelectedBid.Price;

            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(amount * 100),  
                Currency = "egp",
                CaptureMethod = "manual",
                PaymentMethodTypes = new List<string> { "card" },
                Metadata = new Dictionary<string, string>
            {
                { "request_id",    dto.RequestId.ToString() },
                { "customer_id",   customer.Id.ToString() },
                { "technician_id", technician.Id.ToString() }
            }
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);

            var escrow = EscrowTransaction.Create(
                serviceRequestId: dto.RequestId,
                customerId: customer.Id,
                technicianId: technician.Id,
                amount: amount,
                platformFeePercent: PlatformFeePercent,
                stripePaymentIntentId: paymentIntent.Id
            );
            escrow.MarkCreated(customerUserId);

            await _escrowRepo.AddAsync(escrow);
            await _escrowRepo.SaveChangesAsync();

            return new CreatePaymentResultDto
            {
                ClientSecret = paymentIntent.ClientSecret,
                EscrowId = escrow.Id,
                Amount = amount
            };
        }

        public async Task ReleasePaymentAsync(int requestId, string customerUserId)
        {
            var escrow = await _escrowRepo.GetByRequestIdAsync(requestId);
            if (escrow == null)
                throw new Exception("Escrow not found.");
            if (escrow.Status != EscrowStatus.Held)
                throw new Exception("Payment is not in held state.");
            if (escrow.Customer.UserId != customerUserId)
                throw new Exception("Unauthorized.");

            var service = new PaymentIntentService();
            await service.CaptureAsync(escrow.ProviderRef);

        }

        public async Task RefundPaymentAsync(int requestId, string customerUserId)
        {
            var escrow = await _escrowRepo.GetByRequestIdAsync(requestId);
            if (escrow == null)
                throw new Exception("Escrow not found.");
            if (escrow.Status != EscrowStatus.Held)
                throw new Exception("Cannot refund — payment is not in held state.");
            if (escrow.Customer.UserId != customerUserId)
                throw new Exception("Unauthorized.");

            var options = new RefundCreateOptions
            {
                PaymentIntent = escrow.ProviderRef,
                Reason = RefundReasons.RequestedByCustomer
            };

            var refundService = new RefundService();
            await refundService.CreateAsync(options);

        }

        public async Task HandlePaymentHeldAsync(string paymentIntentId, string chargeId)
        {
            var escrow = await _escrowRepo.GetByProviderRefAsync(paymentIntentId);
            if (escrow == null || escrow.Status != EscrowStatus.PendingDeposit)
                return;

            escrow.MarkDeposited(chargeId);
            await _escrowRepo.SaveChangesAsync();

            // Update request status only if still assigned.
            var request = await _requestRepo.GetByIdAsync(escrow.ServiceRequestId);
            if (request != null && request.Status == Status.assigned)
            {
                request.Start();
                await _requestRepo.SaveChangesAsync();
            }
        }

        public async Task HandlePaymentReleasedAsync(string paymentIntentId)
        {
            var escrow = await _escrowRepo.GetByProviderRefAsync(paymentIntentId);
            if (escrow == null || escrow.Status != EscrowStatus.Held)
                return;

            escrow.MarkReleased();
            await _escrowRepo.SaveChangesAsync();

            await _walletService.CreditAsync(
                userId: escrow.Technician.UserId,
                amount: escrow.TechnicianPayout,
                description: $"أرباح طلب رقم #{escrow.ServiceRequestId}",
                type: WalletTransactionType.payout,
                escrowId: escrow.Id
            );

            var request = await _requestRepo.GetByIdAsync(escrow.ServiceRequestId);
            request?.MarkCompleted();
            await _requestRepo.SaveChangesAsync();

          

            
        }

        public async Task HandlePaymentRefundedAsync(string paymentIntentId)
        {
            var escrow = await _escrowRepo.GetByProviderRefAsync(paymentIntentId);
            if (escrow == null) return;

            escrow.MarkRefunded();
            await _escrowRepo.SaveChangesAsync();

            await _walletService.CreditAsync(
                userId: escrow.Customer.UserId,
                amount: escrow.Amount,
                description: $"استرداد طلب رقم #{escrow.ServiceRequestId}",
                type: WalletTransactionType.refund,
                escrowId: escrow.Id
            );

          
        }

        public async Task<EscrowDto?> GetEscrowByRequestAsync(int requestId)
        {
            var escrow = await _escrowRepo.GetByRequestIdAsync(requestId);
            return escrow == null ? null : EscrowMapper.ToDto(escrow);
        }
    }
}
