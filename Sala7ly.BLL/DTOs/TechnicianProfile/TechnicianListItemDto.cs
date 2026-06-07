namespace Sala7ly.BLL.DTOs.TechnicianDTOs
{
    public class TechnicianListItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public double OverallRating { get; set; }
        public int CompletedJobs { get; set; }
        public bool IsApproved { get; set; }
        public bool IsFeatured { get; set; }
        public string SubscriptionTier { get; set; }
    }
}