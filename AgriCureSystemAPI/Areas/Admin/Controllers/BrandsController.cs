using AgriCureSystemAPI.DTOs.Requests;
using AgriCureSystemAPI.Models;
using AgriCureSystemAPI.Repositories.IRepositories;
using AgriCureSystemAPI.Utility;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriCureSystemAPI.Areas.Admin.Controllers
{
    [Route("api[area]/[controller]")]
    [ApiController]
    [Area("Admin")]
    [Authorize(Roles = $"{SD.SuperAdmin},{SD.Admin}")]

    public class BrandsController : ControllerBase
    {

        private IBrandRepository _brandRepository;// = new BrandRepository();

        public BrandsController(IBrandRepository brandRepository)
        {
            _brandRepository = brandRepository;
        }

        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var brands = await _brandRepository.GetAsync(includeProperties: "Products");


            var statistics = new
            {
                TotalCount = brands.Count(),
                ActiveCount = brands.Count(c => c.Status == true),

                Data = brands.Select(b => new
                {
                    b.Id, 
                    b.Name,
                    b.Status,

                    ProductsCount = b.Products != null ? b.Products.Count() : 0
                })
            };

            return Ok(statistics);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create(BrandRequest brandRequest)
        {
            await _brandRepository.CreateAsync(brandRequest.Adapt<Brand>());
            await _brandRepository.CommitAsync();

            return Ok("Add Brand Successfully");
        }

        [HttpGet("Details/{id}")]

        public async Task<IActionResult> Details([FromRoute] int id)
        {
            var brand = await _brandRepository.GetOneAsync(e => e.Id == id);

            if (brand is not null)
            {
                return Ok(brand);
            }

            return NotFound();
        }

        [HttpPut("Edit/{id}")]

        public async Task<IActionResult> Edit(int id, BrandRequest brandRequest)
        {

            var brandInDb = await _brandRepository.GetOneAsync(e => e.Id == id);

            if (brandInDb is null)
                return NotFound();

            brandInDb.Name = brandRequest.Name;
            brandInDb.Description = brandRequest.Description;
            brandInDb.Status = brandRequest.Status;


            await _brandRepository.CommitAsync();

            return Ok("Update Brand Successfully");
        }
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var brand = await _brandRepository.GetOneAsync(e => e.Id == id);

            if (brand is not null)
            {
                _brandRepository.Delete(brand);
                await _brandRepository.CommitAsync();

                return Ok("Delete Brand Successfully");
            }

            return NotFound();

        }
    }
}
