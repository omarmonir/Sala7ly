using System;

namespace Sala7ly.DAL.Entities
{
    public class TechnicianPortfolio : BaseEntity
    {
        public int TechnicianId { get; private set; }       
        public int? ServiceRequestId { get; private set; }         // from service request

        public string ImageUrl { get; private set; }
        public string Caption { get; private set; }
        public string Type { get; private set; }           
        public DateTime UploadedAt { get; private set; }











        // navigation
        public ServiceRequest ServiceRequest { get; private set; }
        public TechnicianProfile Technician { get; private set; }
    }
}