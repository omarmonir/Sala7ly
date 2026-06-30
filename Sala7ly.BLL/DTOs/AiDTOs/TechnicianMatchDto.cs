namespace Sala7ly.BLL.DTOs.AiDTOs
{
    public class TechnicianMatchDto
    {
        public int TechnicianId { get; set; }
        public string TechnicianName { get; set; } = string.Empty;
        public double OverallRating { get; set; }
        public int CompletedJobs { get; set; }
        public string? ReviewSummary { get; set; }
        public double? SentimentScore { get; set; }
        public double FinalScore { get; set; }
        public string MatchReason { get; set; } = string.Empty;
    }
}
