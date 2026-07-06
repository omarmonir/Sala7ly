namespace Sala7ly.BLL.DTOs.WalletDTOs
{
    public class TopUpResultDto
    {
        public string ClientSecret { get; set; }
        public string PaymentIntentId { get; set; }
        public string CheckoutUrl { get; set; }
        public string SessionId { get; set; }
        public decimal Amount { get; set; }
    }
}
