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

        // FIX: address fields were missing — these are now populated in the mapper
        // so Assigned Tasks, My Requests, and Open Requests all show the real address.
        public int AddressId { get; set; }
        public string? Address { get; set; }   // "Street, District, City"
    }
}