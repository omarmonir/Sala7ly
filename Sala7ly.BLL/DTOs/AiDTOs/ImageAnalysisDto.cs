namespace Sala7ly.BLL.DTOs.AiDTOs
{
    public class ImageAnalysisDto
    {
        public string DetectedProblem { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public List<string> AffectedComponents { get; set; } = new();
        public string SuggestedDescription { get; set; } = string.Empty;
        public float Confidence { get; set; }

        // Not from AI — set by the service
        public int LatencyMs { get; set; }
    }
}
