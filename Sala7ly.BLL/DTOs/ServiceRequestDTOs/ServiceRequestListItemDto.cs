namespace Sala7ly.BLL.DTOs.ServiceRequestDTOs
{
    public class ServiceRequestListItemDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public string Urgency { get; set; }
        public bool IsEmergency { get; set; }
        public DateTime ScheduledAt { get; set; }
        public int CategoryId { get; set; }
        public string? CustomerName { get; set; }
    }
}
