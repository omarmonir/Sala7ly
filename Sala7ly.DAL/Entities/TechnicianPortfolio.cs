using System;

namespace Sala7ly.DAL.Entities
{
    public class TechnicianPortfolio : BaseEntity
    {
        public int TechnicianId { get;  set; }       
        public int? ServiceRequestId { get;  set; }         // from service request

        public string ImageUrl { get;  set; }
        public string Caption { get;  set; }
        public string Type { get;  set; }           
        public DateTime UploadedAt { get;  set; }











        // navigation
        public ServiceRequest ServiceRequest { get;  set; }
        public TechnicianProfile Technician { get; set; }
    }
}