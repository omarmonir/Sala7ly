using Sala7ly.BLL.DTOs.AddressDTOs;
using Sala7ly.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sala7ly.BLL.Mapper
{
    public static class AddressMapper
    {
        public static AddressDto ToDto(Address address)
            => new AddressDto
            {
                Id = address.Id,
                Street = address.Street,
                City = address.City,
                District = address.District,
            };

        public static Address ToEntity(CreateAddressDto dto)
            => new Address(
                dto.CustomerId,
                dto.Street,
                dto.City,
                dto.District
                );
                

    }
}
