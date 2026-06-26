using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sala7ly.BLL.DTOs.ReviewDTOs;
using Sala7ly.BLL.Mapper;
using Sala7ly.BLL.Services.Abstraction;
using Sala7ly.DAL.Entities;
using Sala7ly.DAL.Enums;
using Sala7ly.DAL.Repositories.Abstraction;

namespace Sala7ly.BLL.Services.Implementation
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepo;
        private readonly IServiceRequestRepository _requestRepo;
        private readonly INotificationService _notificationService;

        public ReviewService(
            IReviewRepository reviewRepo,
            IServiceRequestRepository requestRepo,
            INotificationService notificationService)
        {
            _reviewRepo = reviewRepo;
            _requestRepo = requestRepo;
            _notificationService = notificationService;
        }

        public async Task<ReviewResponseDto?> GetByIdAsync(int id)
        {
            var review = await _reviewRepo.GetByIdAsync(id);
            return review is null ? null : ReviewMapper.ToResponseDto(review);
        }

        public async Task<ReviewResponseDto?> GetByRequestIdAsync(int requestId)
        {
            var review = await _reviewRepo.GetByRequestIdAsync(requestId);
            return review is null ? null : ReviewMapper.ToResponseDto(review);
        }

        public async Task<List<ReviewResponseDto>> GetForTechnicianAsync(string technicianUserId)
        {
            var reviews = await _reviewRepo.GetByRevieweeIdAsync(technicianUserId);
            return reviews.Select(ReviewMapper.ToResponseDto).ToList();
        }

        public async Task<bool> CreateAsync(string reviewerUserId, CreateReviewDto dto)
        {
            // load the request with its parties to figure out who's being reviewed
            var request = await _requestRepo.GetByIdWithPartiesAsync(dto.RequestId);
            if (request is null)
                return false;

            // only completed requests can be reviewed
            if (request.Status != Status.completed)
                return false;

            var customerUserId = request.Profile?.UserId;
            var technicianUserId = request.SelectedBid?.Technician?.UserId;

            // reviewer must be a party to this request; reviewee is the other party
            string? revieweeUserId =
                reviewerUserId == customerUserId ? technicianUserId :
                reviewerUserId == technicianUserId ? customerUserId :
                null;

            if (revieweeUserId is null)
                return false;   // reviewer isn't part of this request

            // prevent duplicate reviews from the same reviewer on this request
            if (await _reviewRepo.HasReviewForRequestAsync(dto.RequestId, reviewerUserId))
                return false;

            // compute overall as the average of the four sub-scores
            var overall = (dto.QualityScore + dto.PunctualityScore
                         + dto.CommunicationScore + dto.ValueScore) / 4f;

            var review = new Review
            {
                RequestId = dto.RequestId,
                ReviewerID = reviewerUserId,
                RevieweeID = revieweeUserId,
                QualityScore = dto.QualityScore,
                PunctualityScore = dto.PunctualityScore,
                CommunicationScore = dto.CommunicationScore,
                ValueScore = dto.ValueScore,
                OverallScore = overall,
                Comment = dto.Comment,
                CreatedAt = DateTime.UtcNow
            };

            await _reviewRepo.AddAsync(review);
            var saved = await _reviewRepo.SaveChangesAsync();

            if (saved <= 0)
                return false;

            // notify the reviewee they got a review
            await _notificationService.NotifyUserAsync(
                userId: revieweeUserId,
                type: NotificationType.system,
                title: "لديك تقييم جديد ⭐",
                body: "قام أحد المستخدمين بتقييم تعاملك. اضغط لعرض التقييم.",
                actorId: reviewerUserId,
                metadata: $"{{\"requestId\": {dto.RequestId}, \"reviewId\": {review.Id}}}");

            return true;
        }

        public async Task<bool> AddTechnicianReplyAsync(string technicianUserId, TechnicianReplyDto dto)
        {
            var review = await _reviewRepo.GetByIdAsync(dto.ReviewId);
            if (review is null)
                return false;

            // only the person being reviewed can reply
            if (review.RevieweeID != technicianUserId)
                return false;

            review.TechnicianReply = dto.Reply;
            _reviewRepo.Update(review);
            await _reviewRepo.SaveChangesAsync();

            // notify the original reviewer that they got a reply
            await _notificationService.NotifyUserAsync(
                userId: review.ReviewerID,
                type: NotificationType.system,
                title: "رد على تقييمك",
                body: "قام الفني بالرد على تقييمك. اضغط لعرض الرد.",
                actorId: technicianUserId,
                metadata: $"{{\"reviewId\": {review.Id}}}");

            return true;
        }
    }
}