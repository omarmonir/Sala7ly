namespace Sala7ly.BLL.DTOs.AiDTOs
{
    public class TechnicianMatchDto
    {
        public Sala7ly.DAL.Entities.TechnicianProfile Technician { get; set; } = null!;
        public double FinalScore { get; set; }
        public string MatchReason { get; set; } = string.Empty;
    }
}
