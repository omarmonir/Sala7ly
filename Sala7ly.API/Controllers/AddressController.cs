using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sala7ly.BLL.DTOs.AddressDTOs;
using Sala7ly.BLL.Services.Abstraction;
using Sala7ly.DAL.Entities;

namespace Sala7ly.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddressController : ControllerBase
    {
        private readonly IAddressService _addressService;
        public AddressController(IAddressService addressService)
        {
            _addressService = addressService;
        }


        // GET api/addresses?customerProfileId=1
        [HttpGet]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetAll()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var addresses = await _addressService.GetAllByUserIdAsync(userId);
            return Ok(addresses);
        }

        // POST api/addresses
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] CreateAddressDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _addressService.CreateAsync(dto);
            if (!result) return BadRequest(new { Message = "Failed to add address" });

            return StatusCode(201, new { Message = "Address added successfully" });
        }

        // PUT api/addresses/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAddressDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _addressService.UpdateAsync(id, dto);
            if (!result) return NotFound(new { Message = "Address not found" });

            return Ok(new { Message = "Address updated successfully" });
        }

        // DELETE api/addresses/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, [FromQuery] string deletedBy)
        {
            if (string.IsNullOrWhiteSpace(deletedBy))
                return BadRequest(new { Message = "deletedBy is required" });

            var result = await _addressService.DeleteAsync(id, deletedBy);
            if (!result) return NotFound(new { Message = "Address not found" });

            return Ok(new { Message = "Address deleted successfully" });
        }

        // PUT api/addresses/{id}/set-default
        [HttpPut("{id}/set-default")]
        public async Task<IActionResult> SetDefault(int id, [FromQuery] int customerProfileId)
        {
            var result = await _addressService.SetDefaultAsync(id, customerProfileId);
            if (!result) return NotFound(new { Message = "Address not found" });

            return Ok(new { Message = "Default address updated" });
        }
    }
}
