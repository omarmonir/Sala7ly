using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Sala7ly.BLL.DTOs.CustomerDTOs
{
    public class CustomerProfileUpdateDto
    {
        [Required(ErrorMessage = "الاسم مطلوب")]
        public string Name { get; set; }

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [Phone(ErrorMessage = "رقم الهاتف غير صحيح")]
        public string PhoneNumber { get; set; }

        public string ImageUrl { get; set; }

        public string AddressDetails { get; set; }

    }
}


