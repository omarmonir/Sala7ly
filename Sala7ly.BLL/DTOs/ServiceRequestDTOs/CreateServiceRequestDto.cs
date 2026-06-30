using Microsoft.AspNetCore.Http;
using Sala7ly.DAL.Enums;

namespace Sala7ly.BLL.DTOs.ServiceRequestDTOs
{
    public class CreateServiceRequestDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public List<IFormFile>? Images { get; set; }
        public Urgency Urgency { get; set; }
        public BookingMode BookingMode { get; set; }
        public bool IsEmergency { get; set; } = false;
        public DateTime ScheduledAt { get; set; }
        public int CategoryId { get; set; }

        // FIX: AddressId is now nullable.
        // If the frontend sends a valid existing ID it is used directly.
        // If it is null/0, the backend creates a new Address from ServiceAddress below.
        public int? AddressId { get; set; }

        // FIX: free-text address the customer typed in the form.
        // Used to create a real Address row when AddressId is not supplied.
        public string? ServiceAddress { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
    }
}