using System;
using System.Collections.Generic;
using System.Text;

namespace Sala7ly.DAL.Entities
{
    public class CustomerProfile
    {

        public int Id { get; set; }

        public int TotalRequests { get; set; }
        public int TotalReviews { get; set; }
        public decimal TotalSpent { get; set; }
        //public bool IsBusinessAccount { get; set; } = false;
        public int UserId { get; set; }











        public User User { get; set; }
    }
}
