using Sala7ly.BLL.DTOs.ServiceRequestDTOs;
using Sala7ly.DAL.Entities;

namespace Sala7ly.BLL.Mapper
{
    public static class ServiceRequestMapper
    {
        // ── helper ────────────────────────────────────────────────────────────

        /// <summary>
        /// Builds "Street, District, City" from a real Address entity,
        /// skipping any null / whitespace parts.
        /// </summary>
        private static string? FormatAddress(Address? address)
        {
            if (address is null) return null;
            var parts = new[] { address.Street, address.District, address.City }
                        .Where(p => !string.IsNullOrWhiteSpace(p));
            return string.Join(", ", parts);
        }

        // ── Entity → List DTO ─────────────────────────────────────────────────

        public static ServiceRequestListItemDto ToListItemDto(ServiceRequest request) =>
            new ServiceRequestListItemDto
            {
                Id = request.Id,
                Title = request.Title,
                Status = request.Status.ToString(),
                Urgency = request.Urgency.ToString(),
                IsEmergency = request.IsEmergency,
                ScheduledAt = request.ScheduledAt,
                CategoryId = request.CategoryId,
                CustomerName = request.Profile?.User?.Name,

                // FIX: was completely missing — Assigned Tasks page showed no address.
                // .Address nav-prop is now included in all repository queries (see repo fix).
                AddressId = request.AddressId,
                Address = FormatAddress(request.Address),
            };

        // ── Entity → Details DTO ──────────────────────────────────────────────

        public static ServiceRequestDetailsDto ToDetailsDto(ServiceRequest request) =>
            new ServiceRequestDetailsDto
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
                CreatedOn = request.CreatedOn,
                CustomerName = request.Profile?.User?.Name,
                CategoryName = request.Category?.NameAr,

                // FIX: was `request.Address?.Street` which returned "s".
                // Now returns "Street, District, City" from the real columns.
                Address = FormatAddress(request.Address),
            };
    }
}