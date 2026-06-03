using System;

namespace Sala7ly.DAL.Entities
{
    public class Address
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }   

        public string Street { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public DateTime CreatedAt { get; set; }






        // navigation
        public CustomerProfile Customer { get; set; }
    }
}