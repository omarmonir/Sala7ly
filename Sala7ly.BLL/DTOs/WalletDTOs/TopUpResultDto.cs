namespace Sala7ly.BLL.DTOs.WalletDTOs
{
    public class TopUpResultDto
    {
        public string ClientSecret { get; set; }
        public string PaymentIntentId { get; set; }
        public decimal Amount { get; set; }
    }
}
