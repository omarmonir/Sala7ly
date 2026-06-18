using Sala7ly.BLL.DTOs.BidDTOs;

namespace Sala7ly.BLL.Services.Abstraction
{
    public interface IBidService
    {
        Task<BidDto> SubmitBidAsync(int requestId, SubmitBidDto dto, string technicianUserId);
        Task AcceptBidAsync(int bidId, string customerUserId);
        Task RejectBidAsync(int bidId, string customerUserId);
        Task WithdrawBidAsync(int bidId, string technicianUserId);
        Task<List<BidDto>> GetBidsByRequestAsync(int requestId);
        Task<BidDto?> GetBidByIdAsync(int bidId);
        Task ExpireOldBidsAsync();
    }
}
