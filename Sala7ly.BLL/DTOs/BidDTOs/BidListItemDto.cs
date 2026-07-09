namespace Sala7ly.BLL.DTOs.BidDTOs
{
    public class BidListItemDto
    {
        public int Id { get; set; }
        public int ServiceRequestId { get; set; }
        public string? ServiceRequestTitle { get; set; }
        public decimal Price { get; set; }
        public int EstimatedDurationMinutes { get; set; }
        public string Status { get; set; }
        public DateTime SubmittedAt { get; set; }
    }
}
