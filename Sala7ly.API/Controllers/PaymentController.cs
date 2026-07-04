using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sala7ly.BLL.DTOs.Common;
using Sala7ly.BLL.DTOs.PaymentDTOs;
using Sala7ly.BLL.Services.Abstraction;
using Stripe;

namespace Sala7ly.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/payments")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IWalletService _walletService;
        private readonly IConfiguration _config;

        public PaymentController(IPaymentService paymentService, IWalletService walletService, IConfiguration config)
        {
            _paymentService = paymentService;
            _walletService = walletService;
            _config = config;
        }

        private string CurrentUserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // POST /api/payments/create
        [Authorize(Roles = "Customer")]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreatePaymentDto dto)
        {
            try
            {
                var result = await _paymentService.CreateEscrowAsync(CurrentUserId, dto);
                return Ok(new ApiResponse<CreatePaymentResultDto> { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<CreatePaymentResultDto> { Success = false, Message = ex.Message });
            }
        }

        // POST /api/payments/release
        [Authorize(Roles = "Customer")]
        [HttpPost("release")]
        public async Task<IActionResult> Release([FromBody] int requestId)
        {
            try
            {
                await _paymentService.ReleasePaymentAsync(requestId, CurrentUserId);
                return Ok(new ApiResponse<string> { Success = true, Message = "جاري تحرير الدفع" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<string> { Success = false, Message = ex.Message });
            }
        }

        // POST /api/payments/refund
        [Authorize(Roles = "Customer")]
        [HttpPost("refund")]
        public async Task<IActionResult> Refund([FromBody] int requestId)
        {
            try
            {
                await _paymentService.RefundPaymentAsync(requestId, CurrentUserId);
                return Ok(new ApiResponse<string> { Success = true, Message = "جاري استرداد المبلغ" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<string> { Success = false, Message = ex.Message });
            }
        }

        // GET /api/payments/{requestId}
        [HttpGet("{requestId}")]
        public async Task<IActionResult> GetEscrow(int requestId)
        {
            try
            {
                var result = await _paymentService.GetEscrowByRequestAsync(requestId);
                if (result == null)
                    return NotFound(new ApiResponse<EscrowDto> { Success = false, Message = "لم يتم العثور على بيانات الدفع" });

                return Ok(new ApiResponse<EscrowDto> { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<EscrowDto> { Success = false, Message = ex.Message });
            }
        }

        // POST /api/webhooks/stripe ← NO [Authorize] here
        [AllowAnonymous]
        [HttpPost("/api/webhooks/stripe")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var signature = Request.Headers["Stripe-Signature"].ToString();

            Event stripeEvent;

            try
            {
                stripeEvent = EventUtility.ConstructEvent(
                    json,
                    signature,
                    _config["Stripe:WebhookSecret"]
                );
            }
            catch (StripeException)
            {
                return BadRequest("Invalid webhook signature.");
            }

            try
            {
                switch (stripeEvent.Type)
                {
                    // Card authorized — money held
                    case "payment_intent.amount_capturable_updated":
                        {
                            var intent = stripeEvent.Data.Object as PaymentIntent;
                            var chargeId = intent?.LatestChargeId ?? string.Empty;
                            await _paymentService.HandlePaymentHeldAsync(intent!.Id, chargeId);
                            break;
                        }

                    // Payment captured — money released to technician
                    case "payment_intent.succeeded":
                        {
                            var intent = stripeEvent.Data.Object as PaymentIntent;
                            if (intent == null)
                                break;

                            var paymentType = intent.Metadata?.ContainsKey("payment_type") == true
                                ? intent.Metadata["payment_type"]
                                : null;

                            if (paymentType == "wallet_topup")
                            {
                                await _walletService.HandleStripePaymentIntentSucceededAsync(intent);
                            }
                            else
                            {
                                await _paymentService.HandlePaymentReleasedAsync(intent.Id);
                            }
                            break;
                        }

                    // Payment refunded
                    case "charge.refunded":
                        {
                            var charge = stripeEvent.Data.Object as Charge;
                            if (charge == null)
                                break;

                            await _paymentService.HandlePaymentRefundedAsync(charge.PaymentIntentId);
                            await _walletService.HandleStripeChargeRefundedAsync(charge);
                            break;
                        }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
