using Microsoft.AspNetCore.SignalR;
using Sala7ly.API.Hubs;
using Sala7ly.BLL.DTOs.BidDTOs;
using Sala7ly.BLL.Mapper;
using Sala7ly.BLL.Services.Abstraction;
using Sala7ly.DAL.Enums;
using Sala7ly.DAL.Repositories.Abstraction;

namespace Sala7ly.BLL.Services.Implementation
{
    public class BidService : IBidService
    {
        private readonly IBidRepository _bidRepo;
        private readonly IServiceRequestRepository _requestRepo;
        private readonly ITechnicianProfileRepository _technicianRepo;
        private readonly IHubContext<BiddingHub> _biddingHub;

        public BidService(
            IBidRepository bidRepo,
            IServiceRequestRepository requestRepo,
            ITechnicianProfileRepository technicianRepo,
            IHubContext<BiddingHub> biddingHub)
        {
            _bidRepo = bidRepo;
            _requestRepo = requestRepo;
            _technicianRepo = technicianRepo;
            _biddingHub = biddingHub;
        }

        // ── Submit Bid ────────────────────────────────────────
        public async Task<BidDto> SubmitBidAsync(int requestId, SubmitBidDto dto, string technicianUserId)
        {
            var request = await _requestRepo.GetByIdAsync(requestId);
            if (request == null)
                throw new Exception("Request not found.");
            if (request.Status != Status.open)
                throw new Exception("Request is no longer accepting bids.");

            var technician = await _technicianRepo.GetByUserIdAsync(technicianUserId);
            if (technician == null || !technician.IsApproved)
                throw new Exception("Technician is not approved.");

            var alreadyBid = await _bidRepo.HasTechnicianBidAsync(requestId, technician.Id);
            if (alreadyBid)
                throw new Exception("You already submitted a bid on this request.");

            // Use mapper to create entity
            var bid = BidMapper.ToEntity(dto, technician.Id, requestId);

            await _bidRepo.AddAsync(bid);
            await _bidRepo.SaveChangesAsync();

            // Reload with navigation to fill technician data
            var saved = await _bidRepo.GetByIdWithDetailsAsync(bid.Id);

            var bidDto = BidMapper.ToDto(saved!);
            // 6. Push to customer via SignalR
            await _biddingHub.Clients
                .Group($"request-{requestId}")
                .SendAsync("NewBidReceived", bidDto);

            // 7. Update bid count
            var count = await _bidRepo.CountByRequestAsync(requestId);
            await _biddingHub.Clients
                .Group($"request-{requestId}")
                .SendAsync("BidCountUpdated", new { requestId, count });

            

            return bidDto;
        }

        // ── Accept Bid ────────────────────────────────────────
        public async Task AcceptBidAsync(int bidId, string customerUserId)
        {
            // 1. Get bid with full details
            var bid = await _bidRepo.GetByIdWithDetailsAsync(bidId);
            if (bid == null)
                throw new Exception("Bid not found.");
            if (bid.Status != BidStatus.pending)
                throw new Exception("Bid is no longer available.");

            // 2. Verify customer owns this request
            if (bid.ServiceRequest.Profile.UserId != customerUserId)
                throw new Exception("Unauthorized.");

            // 3. Verify request is still open
            if (bid.ServiceRequest.Status != Status.open)
                throw new Exception("Request is no longer open.");

            // 4. Accept this bid via domain method
            bid.Accept();

            // 5. Update request via domain method
            bid.ServiceRequest.AssignBid(bidId);

            await _bidRepo.SaveChangesAsync();

            // 6. Reject all other pending bids
            var otherBids = await _bidRepo
                .GetPendingByRequestAsync(bid.ServiceRequestId, bidId);

            foreach (var other in otherBids)
                other.Reject();

            await _bidRepo.SaveChangesAsync();

            // 7. Notify accepted technician via SignalR
            await _biddingHub.Clients
                .User(bid.Technician.UserId)
                .SendAsync("BidAccepted", new
                {
                    bidId,
                    requestId = bid.ServiceRequestId
                });

          
        }

        // ── Reject Bid ────────────────────────────────────────
        public async Task RejectBidAsync(int bidId, string customerUserId)
        {
            var bid = await _bidRepo.GetByIdWithDetailsAsync(bidId);
            if (bid == null)
                throw new Exception("Bid not found.");

            if (bid.ServiceRequest.Profile.UserId != customerUserId)
                throw new Exception("Unauthorized.");

            bid.Reject();
            await _bidRepo.SaveChangesAsync();

            await _biddingHub.Clients
                .User(bid.Technician.UserId)
                .SendAsync("BidRejected", new { bidId });

           
        }

        // ── Withdraw Bid ──────────────────────────────────────
        public async Task WithdrawBidAsync(int bidId, string technicianUserId)
        {
            var bid = await _bidRepo.GetByIdWithDetailsAsync(bidId);
            if (bid == null)
                throw new Exception("Bid not found.");

            if (bid.Technician.UserId != technicianUserId)
                throw new Exception("Unauthorized.");

            bid.Withdraw();
            await _bidRepo.SaveChangesAsync();

            // Notify customer via SignalR
            await _biddingHub.Clients
                .Group($"request-{bid.ServiceRequestId}")
                .SendAsync("BidWithdrawn", new { bidId });

            var count = await _bidRepo.CountByRequestAsync(bid.ServiceRequestId);
            await _biddingHub.Clients
                .Group($"request-{bid.ServiceRequestId}")
                .SendAsync("BidCountUpdated", new
                {
                    requestId = bid.ServiceRequestId,
                    count
                });
        }

        // ── Get Bids by Request ───────────────────────────────
        public async Task<List<BidDto>> GetBidsByRequestAsync(int requestId)
        {
            var bids = await _bidRepo.GetByRequestIdAsync(requestId);
            return BidMapper.ToDtoList(bids);
        }

        // ── Get Single Bid ────────────────────────────────────
        public async Task<BidDto?> GetBidByIdAsync(int bidId)
        {
            var bid = await _bidRepo.GetByIdWithDetailsAsync(bidId);
            return bid == null ? null : BidMapper.ToDto(bid);
        }

        // ── Expire Old Bids (Background Job) ─────────────────
        public async Task ExpireOldBidsAsync()
        {
            var expiredBids = await _bidRepo.GetExpiredBidsAsync();

            foreach (var bid in expiredBids)
                bid.Expire();

            await _bidRepo.SaveChangesAsync();
        }
    }
}
