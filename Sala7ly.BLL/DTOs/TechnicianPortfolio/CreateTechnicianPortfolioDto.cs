namespace Sala7ly.BLL.Dtos.TechnicianPortfolio
{
    public class CreateTechnicianPortfolioDto
    {
        public int TechnicianId { get; set; }
        public int? RequestId { get; set; }
        public string ImageUrl { get; set; }
        public string Caption { get; set; }
        public string Type { get; set; }   
    }
}