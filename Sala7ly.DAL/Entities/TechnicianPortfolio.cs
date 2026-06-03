using System;

namespace Sala7ly.DAL.Entities
{
    public class TechnicianPortfolio
    {
        public int Id { get; set; }

        public int TechnicianId { get; set; }       
        public int? RequestId { get; set; }         // from service request

        public string ImageUrl { get; set; }
        public string Caption { get; set; }
        public string Type { get; set; }           
        public DateTime UploadedAt { get; set; }












        // navigation
        public TechnicianProfile Technician { get; set; }
    }
}