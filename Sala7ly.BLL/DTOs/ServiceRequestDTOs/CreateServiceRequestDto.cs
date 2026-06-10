using Sala7ly.DAL.Enums;

namespace Sala7ly.BLL.DTOs.ServiceRequestDTOs
{
    public class CreateServiceRequestDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public List<string> ImageUrls { get; set; }  // for photos of the issue
        public Urgency Urgency { get; set; }
        public BookingMode BookingMode { get; set; }
        public bool IsEmergency { get; set; } = false;
        public DateTime ScheduledAt { get; set; }  // when customer wants the service
        public int AddressId { get; set; }
        public int CategoryId { get; set; }
    }
}
