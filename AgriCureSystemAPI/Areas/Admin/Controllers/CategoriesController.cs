using AgriCureSystemAPI.DTOs.Requests;
using AgriCureSystemAPI.Models;
using AgriCureSystemAPI.Repositories.IRepositories;
using AgriCureSystemAPI.Utility;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace AgriCureSystemAPI.Areas.Admin.Controllers
{
    [Route("api/[area]/[controller]")]
    [Area("Admin")]
    [ApiController]

    [Authorize(Roles = $"{SD.SuperAdmin},{SD.Admin}")]

    public class CategoriesController : ControllerBase
    {
        private ICategoryRepository _categoryRepository;

        public CategoriesController(ICategoryRepository categoryRepository )
        {
            _categoryRepository = categoryRepository;
        }
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryRepository.GetAsync();

            var statistics = new
            {
                TotalCount = categories.Count(),
                ActiveCount = categories.Count(c => c.Status == true), 
                Data = categories 
            };

            return Ok(statistics);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create(CategoryRequest categoryRequest)
        {
            await _categoryRepository.CreateAsync(categoryRequest.Adapt<Category>());

            await _categoryRepository.CommitAsync();

            return Ok("Add Category Successfully");
        }

        [HttpPut("Edit/{id}")]
        public async Task<IActionResult> Edit(int id, CategoryRequest categoryRequest)
        {

            var categoryInDb = await _categoryRepository.GetOneAsync(e => e.Id == id);

            if (categoryInDb is null)
                return NotFound();

            categoryInDb.Name = categoryRequest.Name;
            categoryInDb.Description = categoryRequest.Description;
            categoryInDb.Status = categoryRequest.Status;


            await _categoryRepository.CommitAsync();

            return Ok("Update Category Successfully");
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var category = await _categoryRepository.GetOneAsync(e => e.Id == id);

            if (category is not null)
            {
                _categoryRepository.Delete(category);
                await _categoryRepository.CommitAsync();

                return Ok("Delete Category Successfully");
            }

            return NotFound();

        }

    }
}
