using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sala7ly.BLL.DTOs.ServicesCategoryDTOs;
using Sala7ly.BLL.Services.Abstraction;

namespace Sala7ly.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceCategoryController : ControllerBase
    {

        private readonly IServiceCategoryService _categoryService;

        public ServiceCategoryController(IServiceCategoryService categoryService)
        {
            _categoryService = categoryService;
        }


        // ── GET api/servicecategory 
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _categoryService.GetAllAsync();
            return Ok(categories);
        }


        // ── GET api/servicecategory/{id} 
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);

            if (category is null)
                return NotFound(new { Message = "Category not found" });

            return Ok(category);
        }


        // ── POST api/servicecategory 
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] CategoryCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _categoryService.AddAsync(dto);
            return StatusCode(201, new { Message = "Category created successfully" });
        }



        // ── PUT api/servicecategory/{id} 
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CategoryUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _categoryService.UpdateAsync(id, dto);

            if (!result)
                return NotFound(new { Message = "Category not found" });

            return Ok(new { Message = "Category updated successfully" });
        }




        // ── DELETE api/servicecategory/{id} 
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _categoryService.DeleteAsync(id);

            if (!result)
                return NotFound(new { Message = "Category not found" });

            return Ok(new { Message = "Category deleted successfully" });
        }



    }


}
