using Sala7ly.BLL.DTOs.ServiceRequestDTOs;
using Sala7ly.DAL.Entities;

namespace Sala7ly.BLL.Mapper
{
    public static class ServiceRequestMapper
    {
        // ── Entity → DTO

        public static ServiceRequestListItemDto ToListItemDto(ServiceRequest request) => new ServiceRequestListItemDto
        {
            Id = request.Id,
            Title = request.Title,
            Status = request.Status.ToString(),
            Urgency = request.Urgency.ToString(),
            IsEmergency = request.IsEmergency,
            ScheduledAt = request.ScheduledAt,
            CategoryId = request.CategoryId
        };

        public static ServiceRequestDetailsDto ToDetailsDto(ServiceRequest request) => new ServiceRequestDetailsDto
        {
            Id = request.Id,
            Title = request.Title,
            Description = request.Description,
            ImageUrls = request.ImageUrls,
            Urgency = request.Urgency.ToString(),
            Status = request.Status.ToString(),
            BookingMode = request.BookingMode.ToString(),
            IsEmergency = request.IsEmergency,
            AiPriceMin = request.AiPriceMin,
            AiPriceMax = request.AiPriceMax,
            ScheduledAt = request.ScheduledAt,
            CompletedAt = request.CompletedAt,
            CustomerId = request.CustomerId,
            CategoryId = request.CategoryId,
            AddressId = request.AddressId,
            CreatedOn = request.CreatedOn
        };
    }
}