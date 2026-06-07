using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sala7ly.BLL.DTOs.CustomerDTOs;
using Sala7ly.BLL.Services.Abstraction;

namespace Sala7ly.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class CustomerController : ControllerBase
    {
      
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }


        // ── GET api/customer
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var customers = await _customerService.GetAllAsync();
            return Ok(customers);
        }



        // ── GET api/customer/{id} 
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var customer = await _customerService.GetByIdAsync(id);

            if (customer is null)
                return NotFound(new { Message = "Customer not found" });

            return Ok(customer);
        }

        // ── POST api/customer 

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] CustomerRegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _customerService.AddAsync(dto);

            if (!result)
                return BadRequest(new { Message = "Registration failed" });

            return StatusCode(201, new { Message = "Customer created successfully" });
        }



        // ── PUT api/customer/{id} 
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id,
                                                [FromBody] CustomerProfileUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _customerService.UpdateAsync(id, dto);

            if (!result)
                return NotFound(new { Message = "Customer not found" });

            return Ok(new { Message = "Customer updated successfully" });
        }


        // ── DELETE api/customer/{id} 
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, [FromQuery] string deletedBy)
        {
            if (string.IsNullOrWhiteSpace(deletedBy))
                return BadRequest(new { Message = "deletedBy is required" });

            var result = await _customerService.DeleteAsync(id, deletedBy);

            if (!result)
                return NotFound(new { Message = "Customer not found" });

            return Ok(new { Message = "Customer deleted successfully" });
        }



    }

}
