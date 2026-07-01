using System.Collections.Generic;

namespace Sala7ly.BLL.DTOs.AiDTOs
{
    public class AnalyzeImagesRequestDto
    {
        public List<string> ImageUrls { get; set; } = new();
    }
}