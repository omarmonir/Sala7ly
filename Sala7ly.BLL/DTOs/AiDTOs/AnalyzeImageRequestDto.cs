using Microsoft.AspNetCore.Http;

namespace Sala7ly.BLL.DTOs.AiDTOs
{
    public class AnalyzeImageRequestDto
    {
        public IFormFile Image { get; set; } = null!;
    }
}