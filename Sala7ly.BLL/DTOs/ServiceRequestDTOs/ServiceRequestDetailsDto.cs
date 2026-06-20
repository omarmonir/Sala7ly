using Sala7ly.DAL.Enums;

namespace Sala7ly.BLL.DTOs.ServiceRequestDTOs
{
    public class ServiceRequestDetailsDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<string> ImageUrls { get; set; }
        public string Urgency { get; set; }
        public string Status { get; set; }
        public string BookingMode { get; set; }
        public bool IsEmergency { get; set; }
        public decimal AiPriceMin { get; set; }
        public decimal AiPriceMax { get; set; }
        public DateTime ScheduledAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int CustomerId { get; set; }
        public int CategoryId { get; set; }
        public int AddressId { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? CustomerName { get; set; }
        public string? CategoryName { get; set; }
        public string? Address { get; set; }
    }
}
