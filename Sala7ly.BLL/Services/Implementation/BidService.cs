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
        private readonly INotificationService _notificationService;

        public BidService(
            IBidRepository bidRepo,
            IServiceRequestRepository requestRepo,
            ITechnicianProfileRepository technicianRepo,
            IHubContext<BiddingHub> biddingHub,
            INotificationService notificationService)
        {
            _bidRepo = bidRepo;
            _requestRepo = requestRepo;
            _technicianRepo = technicianRepo;
            _biddingHub = biddingHub;
            _notificationService = notificationService;
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

            // 8. Persistent notification to the customer who owns the request
            await _notificationService.NotifyUserAsync(
                userId: request.Profile.UserId,
                type: NotificationType.new_bid,
                title: "عرض جديد على طلبك",
                body: "قام أحد الفنيين بتقديم عرض على طلبك. اضغط لعرض التفاصيل.",
                actorId: technicianUserId,
                metadata: $"{{\"requestId\": {requestId}, \"bidId\": {bid.Id}}}");

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

            // 8. Persistent notification to the accepted technician
            await _notificationService.NotifyUserAsync(
                userId: bid.Technician.UserId,
                type: NotificationType.bid_accepted,
                title: "تم قبول عرضك 🎉",
                body: "هنّئنا! تم قبول عرضك على أحد الطلبات. اضغط لعرض التفاصيل.",
                actorId: customerUserId,
                metadata: $"{{\"requestId\": {bid.ServiceRequestId}, \"bidId\": {bidId}}}");

            // 9. Persistent notification to each rejected technician
            foreach (var other in otherBids)
            {
                await _notificationService.NotifyUserAsync(
                    userId: other.Technician.UserId,
                    type: NotificationType.bid_accepted,
                    title: "لم يتم اختيار عرضك",
                    body: "تم قبول عرض فني آخر على هذا الطلب. شكراً لمشاركتك.",
                    actorId: customerUserId,
                    metadata: $"{{\"requestId\": {bid.ServiceRequestId}, \"bidId\": {other.Id}}}");
            }
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

            // Persistent notification to the rejected technician
            await _notificationService.NotifyUserAsync(
                userId: bid.Technician.UserId,
                type: NotificationType.new_bid,
                title: "تم رفض عرضك",
                body: "نأسف، تم رفض عرضك على أحد الطلبات.",
                actorId: customerUserId,
                metadata: $"{{\"requestId\": {bid.ServiceRequestId}, \"bidId\": {bidId}}}");
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

            // Persistent notification to the customer who owns the request
            await _notificationService.NotifyUserAsync(
                userId: bid.ServiceRequest.Profile.UserId,
                type: NotificationType.new_bid,
                title: "تم سحب عرض",
                body: "قام أحد الفنيين بسحب عرضه على طلبك.",
                actorId: technicianUserId,
                metadata: $"{{\"requestId\": {bid.ServiceRequestId}, \"bidId\": {bidId}}}");
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
        // Add to BidService.cs

        // ── Update Bid ────────────────────────────────────────────
        public async Task<BidDto> UpdateBidAsync(int bidId, UpdateBidDto dto, string currentUserId)
        {
            var bid = await _bidRepo.GetByIdWithDetailsAsync(bidId);
            if (bid == null)
                throw new Exception("العرض غير موجود.");

            var isAdmin = false; // resolve from role — pass from controller
            var isTechnician = bid.Technician.UserId == currentUserId;

            if (!isTechnician)
                throw new UnauthorizedAccessException("غير مصرح لك بتعديل هذا العرض.");

            // Update editable fields
            bid.Update(dto.Price, dto.ProposalMessage, dto.EstimatedDurationMinutes);

            // Status change — only if provided and bid is not already accepted/rejected
            if (!string.IsNullOrWhiteSpace(dto.Status))
            {
                if (!Enum.TryParse<BidStatus>(dto.Status, ignoreCase: true, out var newStatus))
                    throw new Exception("حالة العرض غير صحيحة.");

                bid.ChangeStatus(newStatus);
            }

            await _bidRepo.SaveChangesAsync();

            var updated = await _bidRepo.GetByIdWithDetailsAsync(bidId);
            var bidDto = BidMapper.ToDto(updated!);

            // Push update to customer via SignalR
            await _biddingHub.Clients
                .Group($"request_{bid.ServiceRequestId}")
                .SendAsync("BidUpdated", bidDto);

            return bidDto;
        }
        // ── Get Technician's Own Bids ─────────────────────────────
        public async Task<List<BidListItemDto>> GetTechnicianBidsAsync(string technicianUserId)
        {
            var technician = await _technicianRepo.GetByUserIdAsync(technicianUserId);
            if (technician == null)
                throw new Exception("الفني غير موجود.");

            var bids = await _bidRepo.GetByTechnicianIdAsync(technician.Id);
            return BidMapper.ToListItemDtoList(bids);
        }

        // ── Get All Bids (Admin) ──────────────────────────────────
        public async Task<List<BidListItemDto>> GetAllBidsAsync()
        {
            var bids = await _bidRepo.GetAllWithDetailsAsync();
            return BidMapper.ToListItemDtoList(bids);
        }
    }
}