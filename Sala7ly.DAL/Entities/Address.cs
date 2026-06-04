using System;
using System.Collections.Generic;

namespace Sala7ly.DAL.Entities
{
    public class Address : BaseEntity
    {
        public int CustomerId { get;  private set; }   

        public string Street { get; private set; }
        public string City { get; private set; }
        public string District { get; private set; }

        // navigation
        public CustomerProfile Customer { get; private set; }
        public ICollection<ServiceRequest> ServiceRequests { get; private set; }
    }
}