using System;

namespace Sala7ly.DAL.Entities
{
    public class TechnicianPortfolio : BaseEntity
    {
        public int TechnicianId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string ImageUrlBefore { get; set; }
        public string ImageUrlAfter { get; set; }
        public DateTime UploadedAt { get; set; }

        // navigation
        public TechnicianProfile Technician { get; set; }
    }
}