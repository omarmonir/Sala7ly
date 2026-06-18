using Microsoft.AspNetCore.Http;

namespace Sala7ly.BLL.Dtos.TechnicianPortfolio
{
    public class CreateTechnicianPortfolioDto
    {
        public int TechnicianId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public IFormFile BeforeImage { get; set; }
        public IFormFile AfterImage { get; set; }
    }
}