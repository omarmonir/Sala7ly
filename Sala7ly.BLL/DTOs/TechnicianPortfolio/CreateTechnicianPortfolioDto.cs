using Microsoft.AspNetCore.Http;

namespace Sala7ly.BLL.Dtos.TechnicianPortfolio
{
    public class CreateTechnicianPortfolioDto
    {
        public int TechnicianId { get; set; }
        public int? ServiceRequestId { get; set; }
        public IFormFile BeforeImage { get; set; }
        public IFormFile AfterImage { get; set; }
        public string Caption { get; set; }
    }
}