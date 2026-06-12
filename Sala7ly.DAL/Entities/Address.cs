using System;
using System.Collections.Generic;

namespace Sala7ly.DAL.Entities
{
    public class Address : BaseEntity
    {
        public int CustomerId { get;  private set; }   
        public string Title { get; private set; } //home, work

        public string Street { get; private set; }
        public string City { get; private set; }
        public string District { get; private set; }

        public bool IsDefault { get; private set; }

        public Address() { }

        public Address(int customerId, string street, string city, string district)
        {
            CustomerId = customerId;
            Street = street;
            City = city;
            District = district;
            IsDefault = false;
        }

        public void UpdateAddress(string street, string city, string district)
        {
            Street = street;
            City = city;
            District = district;
        }

        public void SetDefault(bool isDefault)
        {
            IsDefault = isDefault;
        }



        // navigation
        public CustomerProfile Customer { get; private set; }
        public ICollection<ServiceRequest> ServiceRequests { get; private set; }
    }
}