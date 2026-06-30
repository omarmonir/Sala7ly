using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Sala7ly.BLL.DTOs.AiDTOs;
using Sala7ly.BLL.Services.Abstraction;
using Sala7ly.DAL.Repositories.Abstraction;

namespace Sala7ly.BLL.Services.Implementation
{
    public class DisputeAnalysisService : BaseAiService, IDisputeAnalysisService
    {
        private readonly IDisputeRepository _disputeRepo;

        public DisputeAnalysisService(
            IGitHubAiClient ai,
            IConfiguration config,
            IDisputeRepository disputeRepo)
            : base(ai, config)
        {
            _disputeRepo = disputeRepo;
        }

        public async Task<DisputeAnalysisDto> AnalyzeAsync(int disputeId)
        {
            var dispute = await _disputeRepo.GetByIdAsync(disputeId);
            if (dispute == null)
                throw new KeyNotFoundException("النزاع غير موجود.");

            var requestDetails = dispute.ServiceRequest is not null
                ? $"الطلب #{dispute.ServiceRequest.Id}: {dispute.ServiceRequest.Title}. الوصف: {dispute.ServiceRequest.Description}"
                : "لا توجد تفاصيل الطلب المتاحة.";

            var customerClaim = dispute.Reason;
            var technicianClaim = "لم يتم تقديم ادعاء فني صريح.";

            var prompt = PromptBuilder.DisputeAnalysisUser(
                customerClaim,
                technicianClaim,
                requestDetails);

            var raw = await CallAsync(HaikuModel, prompt, systemPrompt: null, maxTokens: 300);

            try
            {
                return ParseJson<DisputeAnalysisDto>(raw);
            }
            catch
            {
                return new DisputeAnalysisDto
                {
                    CaseSummary = "تعذّر تحليل هذا النزاع الآن.",
                    Timeline = new List<string>(),
                    CustomerStrength = 0.5,
                    TechnicianStrength = 0.5,
                    Recommendation = "no_refund",
                    RecommendedAmount = 0,
                    Reasoning = "لم يتمكّن النظام من تقديم تقييم موثوق لهذه الحالة." 
                };
            }
        }
    }
}
