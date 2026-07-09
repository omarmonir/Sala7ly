using System.Text.Json.Serialization;

namespace Sala7ly.BLL.DTOs.AiDTOs
{
    /// <summary>
    /// AI vision-analysis result. Property names are annotated because the
    /// model is prompted (see PromptBuilder.ImageAnalysisUser) to return
    /// snake_case JSON, which System.Text.Json's case-insensitive matching
    /// alone cannot map onto PascalCase members — previously this DTO had
    /// no attributes at all, so every analysis silently deserialized into
    /// an empty/default object.
    /// </summary>
    public class ImageAnalysisDto
    {
        [JsonPropertyName("detected_problem")]
        public string DetectedProblem { get; set; } = string.Empty;

        [JsonPropertyName("category")]
        public string Category { get; set; } = string.Empty;

        [JsonPropertyName("severity")]
        public string Severity { get; set; } = string.Empty;

        [JsonPropertyName("affected_components")]
        public List<string> AffectedComponents { get; set; } = new();

        [JsonPropertyName("suggested_description")]
        public string SuggestedDescription { get; set; } = string.Empty;

        [JsonPropertyName("confidence")]
        public float Confidence { get; set; }

        // Not from AI — set by the service after the call completes.
        [JsonIgnore]
        public int LatencyMs { get; set; }
    }
}