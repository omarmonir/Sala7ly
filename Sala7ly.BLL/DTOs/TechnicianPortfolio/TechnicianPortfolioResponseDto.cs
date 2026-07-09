using System;

namespace Sala7ly.BLL.Dtos.TechnicianPortfolio
{
    public class TechnicianPortfolioResponseDto
    {
        public int Id { get; set; }
        public int TechnicianId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrlBefore { get; set; }
        public string ImageUrlAfter { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}