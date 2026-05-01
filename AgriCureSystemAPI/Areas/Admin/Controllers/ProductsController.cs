using AgriCureSystemAPI.DTOs.Requests;
using AgriCureSystemAPI.Models;
using AgriCureSystemAPI.Repositories.IRepositories;
using AgriCureSystemAPI.Utility;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AgriCureSystemAPI.Areas.Admin.Controllers
{
    [Route("api[area]/[controller]")]
    [ApiController]
    [Area("Admin")]
    [Authorize(Roles = $"{SD.SuperAdmin},{SD.Admin}")]

    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IBrandRepository _brandRepository;


        public ProductsController(IProductRepository productRepository, ICategoryRepository categoryRepository, IBrandRepository brandRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _brandRepository = brandRepository;
        }

        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var products = await _productRepository.GetAsync(includes: [e => e.Category, e => e.Brand]);

            var response = new
            {
                TotalCount = products.Count(),
                ActiveCount = products.Count(p => p.Status == true), 
                Data = products
            };

            return Ok(response);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromForm] CreateProductRequest productRequest)
        {

            var product = productRequest.Adapt<Product>();

            if (productRequest.MainImg is not null && productRequest.MainImg.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(productRequest.MainImg.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", fileName);

                // Save img in wwwroot
                using (var stream = System.IO.File.Create(filePath))
                {
                    await productRequest.MainImg.CopyToAsync(stream);
                }

                // Save img in DB
                product.MainImg = fileName;

                // Save product in DB
                await _productRepository.CreateAsync(product);
                await _categoryRepository.CommitAsync();

                return Created();
            }

            return BadRequest();
        }

        [HttpGet("Details/{productId}")]
        public async Task<IActionResult> Details(int productId)
        {
            var product = await _productRepository.GetOneAsync(e => e.ProductId == productId);

            if (product is not null)
            {
                return Ok(product);
            }

            return NotFound();
        }

        [HttpPut("Edit/{id}")]
        public async Task<IActionResult> Edit(int id, [FromForm] UpdateProductRequest updateProductRequest)
        {
            var productInDB = await _productRepository.GetOneAsync(e => e.ProductId == id, tracked: false);

            var product = updateProductRequest.Adapt<Product>();

            if (productInDB is not null)
            {
                if (updateProductRequest.MainImg is not null && updateProductRequest.MainImg.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(updateProductRequest.MainImg.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", fileName);

                    // Save img in wwwroot
                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await updateProductRequest.MainImg.CopyToAsync(stream);
                    }

                    // Delete old img from wwwroot
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", productInDB.MainImg);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }

                    // Save img in DB
                    product.MainImg = fileName;
                }
                else
                {
                    product.MainImg = productInDB.MainImg;
                }

                // Update img in DB
                _productRepository.Edit(product);
                await _categoryRepository.CommitAsync();

                return NoContent();
            }

            return NotFound();
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var product = await _productRepository.GetOneAsync(e => e.ProductId == id);

            if (product is not null)
            {
                // Delete old img from wwwroot
                var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", product.MainImg);
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }

                _productRepository.Delete(product);
                await _categoryRepository.CommitAsync();

                return NoContent();
            }

            return NotFound();
        }
    }
}
