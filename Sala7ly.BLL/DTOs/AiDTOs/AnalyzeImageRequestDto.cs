namespace Sala7ly.BLL.DTOs.AiDTOs
{
    public class AnalyzeImageRequestDto
    {
        public string Base64Image { get; set; } = string.Empty;
        public string? MediaType { get; set; }    // "image/jpeg" | "image/png" etc.
    }
}
