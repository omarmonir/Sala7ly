using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Sala7ly.BLL.Services.Abstraction;
using Sala7ly.DAL.Repositories.Abstraction;

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Sala7ly.BLL.Services.Abstraction;
using Sala7ly.DAL.Repositories.Abstraction;

namespace Sala7ly.BLL.Services.Implementation
{
    public class InsightsService : BaseAiService, IInsightsService
    {
        private readonly IServiceRequestRepository _requestRepo;
        private readonly ITechnicianProfileRepository _technicianRepo;
        private readonly IReviewRepository _reviewRepo;

        public InsightsService(
            IGitHubAiClient ai,
            IConfiguration config,
            IAiInteractionRepository aiInteractionRepo,
            IServiceRequestRepository requestRepo,
            ITechnicianProfileRepository technicianRepo,
            IReviewRepository reviewRepo)
            : base(ai, config, aiInteractionRepo)
        {
            _requestRepo = requestRepo;
            _technicianRepo = technicianRepo;
            _reviewRepo = reviewRepo;
        }

        public async Task<string> GenerateWeeklyInsightsAsync()
        {
            var requests = (await _requestRepo.GetAllAsync()).ToList();
            var technicians = (await _technicianRepo.GetAllAsync()).ToList();
            var reviews = (await _reviewRepo.GetAllAsync()).ToList();

            var totalRequests = requests.Count;
            var completedRequests = requests.Count(r => r.Status == Sala7ly.DAL.Enums.Status.completed);
            var activeRequests = requests.Count(r => r.Status != Sala7ly.DAL.Enums.Status.completed && r.Status != Sala7ly.DAL.Enums.Status.cancelled);
            var totalTechnicians = technicians.Count;
            var approvedTechnicians = technicians.Count(t => t.IsApproved);
            var totalReviews = reviews.Count;

            var prompt = PromptBuilder.InsightsUser(
                totalRequests,
                completedRequests,
                activeRequests,
                totalTechnicians,
                approvedTechnicians,
                totalReviews);

            var raw = await CallAsync(HaikuModel, prompt, systemPrompt: null, maxTokens: 300);
            return raw.Trim();
        }
    }
}
