using System;
using System.Collections.Generic;
using System.Text;

namespace Sala7ly.DAL.Entities
{
    public class CustomerProfile : BaseEntity
    {

        public int TotalRequests { get; set; }
        public int TotalReviews { get; set; }
        public decimal TotalSpent { get; set; }
        //public bool IsBusinessAccount { get; set; } = false;
        public string UserId { get; set; }

        public User User { get; set; }
        public ICollection<Address> Addresses { get; set; }
        public ICollection<ServiceRequest> ServiceRequests { get; set; }
        public ICollection<FavoriteTechnician> FavoriteTechnicians { get; set; }
    }
}
