using System.Text.Json.Serialization;

namespace Sala7ly.BLL.DTOs.AiDTOs
{
    public class ImageAnalysisResultDto
    {
        [JsonPropertyName("detected_problem")]
        public string DetectedProblem { get; set; }

        [JsonPropertyName("category")]
        public string Category { get; set; }

        [JsonPropertyName("severity")]
        public string Severity { get; set; }

        [JsonPropertyName("affected_components")]
        public List<string> AffectedComponents { get; set; }

        [JsonPropertyName("suggested_description")]
        public string SuggestedDescription { get; set; }

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }
    }
}
