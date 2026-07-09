using System.Text.Json.Serialization;

namespace Sala7ly.BLL.DTOs.AiDTOs
{
    public class PriceEstimateDto
    {
        [JsonPropertyName("min_price")]
        public decimal MinPrice { get; set; }

        [JsonPropertyName("max_price")]
        public decimal MaxPrice { get; set; }

        [JsonPropertyName("fair_price")]
        public decimal FairPrice { get; set; }

        [JsonPropertyName("price_factors")]
        public List<string> PriceFactors { get; set; }

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }
    }
}
