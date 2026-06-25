namespace Sala7ly.BLL.DTOs.WalletDTOs
{
    public class WalletTransactionDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string? Reference { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
