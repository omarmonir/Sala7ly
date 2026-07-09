namespace Sala7ly.BLL.DTOs.AiDTOs
{
    /// <summary>
    /// Returned by IMatchingService.FindMatchesAsync.
    /// Uses a safe projection instead of the raw EF entity to avoid
    /// circular reference serialization and internal data exposure.
    /// </summary>
    public class TechnicianMatchDto
    {
        public int TechnicianId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public int ExperienceYears { get; set; }
        public double OverallRating { get; set; }
        public int TotalReviews { get; set; }
        public int CompletedJobs { get; set; }
        public List<string> CategoryNames { get; set; } = new();

        public double FinalScore { get; set; }
        public string MatchReason { get; set; } = string.Empty;
    }
}