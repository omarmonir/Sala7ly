using System.Text.Json.Serialization;

namespace Sala7ly.BLL.DTOs.AiDTOs
{
    public class DisputeAnalysisDto
    {
        [JsonPropertyName("case_summary")]
        public string CaseSummary { get; set; }

        [JsonPropertyName("timeline")]
        public List<string> Timeline { get; set; }

        [JsonPropertyName("customer_position_strength")]
        public double CustomerStrength { get; set; }

        [JsonPropertyName("technician_position_strength")]
        public double TechnicianStrength { get; set; }

        [JsonPropertyName("recommendation")]
        public string Recommendation { get; set; }

        [JsonPropertyName("recommended_amount")]
        public decimal RecommendedAmount { get; set; }

        [JsonPropertyName("reasoning")]
        public string Reasoning { get; set; }
    }
}
