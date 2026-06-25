namespace Sala7ly.BLL.DTOs.PaymentDTOs
{
    public class CreatePaymentResultDto
    {
        public string ClientSecret { get; set; }  // sent to frontend for Stripe
        public int EscrowId { get; set; }
        public decimal Amount { get; set; }
    }
}
