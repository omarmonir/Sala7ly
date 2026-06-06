using System;

namespace Sala7ly.BLL.Dtos.TechnicianPortfolio
{
    public class TechnicianPortfolioResponseDto
    {
        public int Id { get; set; }
        public int TechnicianId { get; set; }
        public int? RequestId { get; set; }
        public string ImageUrl { get; set; }
        public string Caption { get; set; }
        public string Type { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}