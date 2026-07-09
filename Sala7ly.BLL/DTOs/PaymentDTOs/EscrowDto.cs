namespace Sala7ly.BLL.DTOs.PaymentDTOs
{
    public class EscrowDto
    {
        public int Id { get; set; }
        public int RequestId { get; set; }
        public decimal Amount { get; set; }
        public decimal PlatformFee { get; set; }
        public decimal TechnicianPayout { get; set; }
        public string Status { get; set; }
        public DateTime? DepositedAt { get; set; }
        public DateTime? ReleasedAt { get; set; }
    }
}
