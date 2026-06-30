using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sala7ly.BLL.DTOs.AiDTOs;
using Sala7ly.BLL.Services.Abstraction;
using Sala7ly.DAL.Repositories.Abstraction;


namespace Sala7ly.API.Controllers
{
    [ApiController]
    [Route("api/ai")]
    [Authorize]
    public class AiController : ControllerBase
    {
        private readonly IRequestRefinerService _refiner;
        private readonly IServiceCategoryRepository _categoryRepo;
        private readonly IPriceEstimationService _priceService;
        private readonly IImageAnalysisService _imageService;
        private readonly IMatchingService _matchingService;
        private readonly IReviewSummaryService _reviewService;
        private readonly IDisputeAnalysisService _disputeService;
        private readonly ISupportChatService _supportChatService;
        private readonly IInsightsService _insightsService;

        public AiController(
            IRequestRefinerService refiner,
            IServiceCategoryRepository categoryRepo,
            IPriceEstimationService priceService,
            IImageAnalysisService imageService,
            IMatchingService matchingService,
            IReviewSummaryService reviewService,
            IDisputeAnalysisService disputeService,
            ISupportChatService supportChatService,
            IInsightsService insightsService)
        {
            _refiner = refiner;
            _categoryRepo = categoryRepo;
            _imageService = imageService;
            _matchingService = matchingService;
            _priceService = priceService;
            _reviewService = reviewService;
            _disputeService = disputeService;
            _supportChatService = supportChatService;
            _insightsService = insightsService;
        }

        // POST /api/ai/ask-followup
        [HttpPost("ask-followup")]
        public async Task<IActionResult> AskFollowUp([FromBody] FollowUpRequestDto dto)
        {
            if (dto.Categories.Count == 0)
                dto.Categories = await GetCategoryListAsync();

            var result = await _refiner.AskFollowUpAsync(dto);
            return Ok(result);
        }

        // POST /api/ai/refine-request
        [HttpPost("refine-request")]
        public async Task<IActionResult> RefineRequest(
            [FromBody] RefineRequestWithAnswersDto dto)
        {
            if (dto.Categories.Count == 0)
                dto.Categories = await GetCategoryListAsync();

            var refineDto = new RefineRequestDto
            {
                RawDescription = dto.RawDescription,
                Categories = dto.Categories
            };

            var result = await _refiner.RefineAsync(refineDto, dto.AllAnswers);
            return Ok(result);
        }
        [HttpPost("price-estimate/{requestId:int}")]
        public async Task<IActionResult> EstimatePrice(int requestId)
        {
            try
            {
                var result = await _priceService.EstimateAsync(requestId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                // Service threw this when GetByIdWithDetailsAsync returned null
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // AI timeout, network error, JSON parse failure, etc.
                return StatusCode(500, new { message = "فشل تقدير السعر.", detail = ex.Message });
            }
        }
        [HttpPost("analyze-image")]
        public async Task<IActionResult> AnalyzeImage([FromBody] AnalyzeImageRequestDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Base64Image))
                    return BadRequest(new { message = "الصورة مطلوبة." });

                var result = await _imageService.AnalyzeImageAsync(
                    dto.Base64Image,
                    dto.MediaType ?? "image/jpeg");

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "فشل تحليل الصورة.", detail = ex.Message });
            }
        }

        [HttpPost("analyze-image/upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AnalyzeImageFromFile([FromForm] AnalyzeImageUploadDto dto)
        {
            try
            {
                if (dto?.File == null)
                    return BadRequest(new { message = "ملف الصورة مطلوب." });

                var result = await _imageService.AnalyzeImageFromFormFileAsync(dto.File);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "فشل تحليل الصورة المرفوعة.", detail = ex.Message });
            }
        }
        [HttpPost("analyze-request-images/{requestId:int}")]
        public async Task<IActionResult> AnalyzeRequestImages(
                 int requestId,
                 [FromBody] AnalyzeRequestImagesDto dto)
        {
            try
            {
                if (dto.ImageUrls == null || !dto.ImageUrls.Any())
                    return BadRequest(new { message = "يجب إرسال رابط صورة واحد على الأقل." });

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var result = await _imageService.AnalyzeMultipleImagesAsync(
                    requestId,
                    dto.ImageUrls,
                    userId);

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "فشل تحليل الصور.", detail = ex.Message });
            }
        }
        [HttpPost("match/{requestId:int}")]
        public async Task<IActionResult> MatchTechnicians(int requestId, [FromQuery] int topN = 10)
        {
            try
            {
                var matches = await _matchingService.FindMatchesAsync(requestId, topN);

                if (!matches.Any())
                    return Ok(new { message = "لا يوجد فنيون متاحون حالياً.", matches });

                return Ok(matches);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "فشلت عملية المطابقة.", detail = ex.Message });
            }
        }
        [HttpPost("support-chat")]
        public async Task<IActionResult> SupportChat([FromBody] SupportChatRequestDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Message))
                return BadRequest(new { message = "الرسالة مطلوبة." });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _supportChatService.ChatAsync(userId ?? string.Empty, dto);
            return Ok(result);
        }

        [HttpPost("review-summary/technician/{technicianId:int}")]
        public async Task<IActionResult> SummarizeTechnicianReview(int technicianId)
        {
            try
            {
                await _reviewService.SummarizeTechnicianAsync(technicianId);
                return Ok(new { message = "تم تحديث ملخص التقييمات." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "فشل تلخيص التقييمات.", detail = ex.Message });
            }
        }

        [HttpPost("dispute-analysis/{disputeId:int}")]
        public async Task<IActionResult> AnalyzeDispute(int disputeId)
        {
            try
            {
                var result = await _disputeService.AnalyzeAsync(disputeId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "فشل تحليل النزاع.", detail = ex.Message });
            }
        }

        [HttpGet("insights/weekly")]
        public async Task<IActionResult> WeeklyInsights()
        {
            try
            {
                var insight = await _insightsService.GenerateWeeklyInsightsAsync();
                return Ok(new { insight });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "فشل توليد رؤى الأداء.", detail = ex.Message });
            }
        }

        private async Task<List<string>> GetCategoryListAsync()
        {
            var cats = await _categoryRepo.GetAllAsync();
            return cats.Select(c => $"{c.Id}:{c.NameAr}").ToList();
        }

    }
}
