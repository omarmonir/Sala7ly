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

        public AiController(IRequestRefinerService refiner, IServiceCategoryRepository categoryRepo)
        {
            _refiner = refiner;
            _categoryRepo = categoryRepo;
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

        private async Task<List<string>> GetCategoryListAsync()
        {
            var cats = await _categoryRepo.GetAllAsync();
            return cats.Select(c => $"{c.Id}:{c.NameAr}").ToList();
        }
    }
}
