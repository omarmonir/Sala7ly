namespace Sala7ly.BLL.DTOs.BidDTOs
{
    public class BidDto
    {
        public int Id { get; set; }
        public int ServiceRequestId { get; set; }
        public int TechnicianId { get; set; }
        public string? TechnicianName { get; set; }
        public string? TechnicianAvatar { get; set; }
        public double TechnicianRating { get; set; }
        public int TechnicianJobs { get; set; }
        public decimal Price { get; set; }
        public int EstimatedDurationMinutes { get; set; }
        public string ProposalMessage { get; set; }
        public string Status { get; set; }
        public DateTime SubmittedAt { get; set; }
        public DateTime? RespondedAt { get; set; }
    }
}
