using Sala7ly.BLL.DTOs.BidDTOs;
using Sala7ly.DAL.Entities;

namespace Sala7ly.BLL.Mapper
{
    public static class BidMapper
    {
        // SubmitBidDto -> Bid entity (use Bid.Create factory)
        public static Bid ToEntity(SubmitBidDto dto, int technicianId, int serviceRequestId)
        {
            return Bid.Create(
                technicianId: technicianId,
                serviceRequestId: serviceRequestId,
                price: dto.Price,
                proposalMessage: dto.ProposalMessage,
                estimatedDurationMinutes: dto.EstimatedDurationMinutes
            );
        }

        // Bid entity -> BidDto
        public static BidDto ToDto(Bid bid)
        {
            return new BidDto
            {
                Id = bid.Id,
                ServiceRequestId = bid.ServiceRequestId,
                TechnicianId = bid.TechnicianId,
                TechnicianName = bid.Technician?.User?.Name,
                TechnicianAvatar = bid.Technician?.User?.ImageUrl,
                TechnicianRating = bid.Technician?.OverallRating ?? 0,
                TechnicianJobs = bid.Technician?.CompletedJobs ?? 0,
                Price = bid.Price,
                EstimatedDurationMinutes = bid.EstimatedDurationMinutes,
                ProposalMessage = bid.ProposalMessage,
                Status = bid.Status.ToString(),
                SubmittedAt = bid.SubmittedAt,
                RespondedAt = bid.RespondedAt
            };
        }

        // List<Bid> -> List<BidDto>
        public static List<BidDto> ToDtoList(IEnumerable<Bid> bids)
        {
            return bids.Select(ToDto).ToList();
        }

        // Bid entity -> BidListItemDto (for technician's bid history)
        public static BidListItemDto ToListItemDto(Bid bid)
        {
            return new BidListItemDto
            {
                Id = bid.Id,
                ServiceRequestId = bid.ServiceRequestId,
                ServiceRequestTitle = bid.ServiceRequest?.Title,
                Price = bid.Price,
                EstimatedDurationMinutes = bid.EstimatedDurationMinutes,
                Status = bid.Status.ToString(),
                SubmittedAt = bid.SubmittedAt
            };
        }
        public static List<BidListItemDto> ToListItemDtoList(IEnumerable<Bid> bids)
        {
            return bids.Select(ToListItemDto).ToList();
        }
    }

}