using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Sala7ly.BLL.DTOs.AddressDTOs
{
    public class UpdateAddressDto
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Street { get; set; }
        [Required]
        public string City { get; set; }
        public string District { get; set; }
        public string UpdatedBy { get; set; }

    }
}
