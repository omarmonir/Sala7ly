using System.Text.Json.Serialization;

namespace Sala7ly.BLL.DTOs.AiDTOs
{
    public class ReviewSummaryResultDto
    {
        [JsonPropertyName("summary")]
        public string Summary { get; set; }

        [JsonPropertyName("strengths")]
        public List<string> Strengths { get; set; }

        [JsonPropertyName("complaints")]
        public List<string> Complaints { get; set; }

        [JsonPropertyName("sentiment_score")]
        public double SentimentScore { get; set; }
    }
}
