using Microsoft.AspNetCore.Http;

namespace Sala7ly.BLL.DTOs.AiDTOs
{
    public class AnalyzeImageUploadDto
    {
        public IFormFile File { get; set; }
    }
}
