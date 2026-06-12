using System;
using System.Collections.Generic;
using System.Text;

namespace Sala7ly.BLL.DTOs.AddressDTOs
{
    public class AddressDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public bool IsDefault { get; set; }


    }
}
