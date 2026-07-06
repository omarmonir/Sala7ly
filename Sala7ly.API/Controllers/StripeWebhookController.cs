using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Sala7ly.BLL.Services.Abstraction;
using Stripe;

namespace Sala7ly.API.Controllers
{
    [ApiController]
    [Route("api/webhook/stripe")]
    public class StripeWebhookController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IWalletService _walletService;

        public StripeWebhookController(IConfiguration config, IWalletService walletService)
        {
            _config = config;
            _walletService = walletService;
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var sigHeader = Request.Headers["Stripe-Signature"].ToString();
            var webhookSecret = _config["Stripe:WebhookSecret"]; // set in configuration

            try
            {
                Event stripeEvent;
                if (!string.IsNullOrEmpty(webhookSecret))
                {
                    stripeEvent = EventUtility.ConstructEvent(json, sigHeader, webhookSecret);
                }
                else
                {
                    // If no webhook secret configured, fallback to parsing without verification (not recommended for production)
                    stripeEvent = EventUtility.ParseEvent(json);
                }

                // handle common event types
                switch (stripeEvent.Type)
                {
                    case "checkout.session.completed":
                    {
                        var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
                        if (session != null && !string.IsNullOrWhiteSpace(session.PaymentIntentId))
                        {
                            var piService = new PaymentIntentService();
                            var pi = await piService.GetAsync(session.PaymentIntentId);
                            if (pi != null)
                                await _walletService.HandleStripePaymentIntentSucceededAsync(pi);
                        }

                        break;
                    }
                    case "payment_intent.succeeded":
                    {
                        var intent = stripeEvent.Data.Object as PaymentIntent;
                        if (intent != null)
                        {
                            await _walletService.HandleStripePaymentIntentSucceededAsync(intent);
                        }
                        break;
                    }
                }

                return Ok();
            }
            catch (StripeException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
