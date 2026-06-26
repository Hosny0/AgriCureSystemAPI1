using AgriCureSystemAPI.DTOs.Requests;
using AgriCureSystemAPI.DTOs.Response;
using AgriCureSystemAPI.Models;
using AgriCureSystemAPI.Repositories.IRepositories;
using AgriCureSystemAPI.Utility;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgriCureSystemAPI.Areas.Admin.Controllers
{
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Area("Admin")]
    [Authorize(Roles = $"{SD.SuperAdmin},{SD.Admin}")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductsController(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IBrandRepository brandRepository,
            UserManager<ApplicationUser> userManager)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _brandRepository = brandRepository;
            _userManager = userManager;
        }

        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var products = await _productRepository.GetAsync(
                includes: [e => e.Category, e => e.Brand, e => e.Reviews]  // ✅ زودنا Reviews
            );

            var data = products.Select(p => new ProductListResponse
            {
                ProductId = p.ProductId,
                Name = p.Name,
                Description = p.Description,
                MainImg = p.MainImg,
                Price = p.Price,
                PriceAfterDiscount = p.PriceAfterDiscount,
                Discount = p.Discount,
                Rate = p.Rate,
                ReviewsCount = p.Reviews.Count,
                CategoryName = p.Category?.Name ?? string.Empty,
                BrandName = p.Brand?.Name ?? string.Empty
            }).ToList();

            var response = new
            {
                TotalCount = data.Count,
                ActiveCount = products.Count(p => p.Status == true),
                Data = data
            };

            return Ok(response);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromForm] CreateProductRequest productRequest)
        {
            if (productRequest.MainImg is null || productRequest.MainImg.Length == 0)
                return BadRequest("Main image is required.");

            var product = productRequest.Adapt<Product>();

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(productRequest.MainImg.FileName);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", fileName);

            using (var stream = System.IO.File.Create(filePath))
            {
                await productRequest.MainImg.CopyToAsync(stream);
            }

            product.MainImg = fileName;

            await _productRepository.CreateAsync(product);
            await _productRepository.CommitAsync(); // ✅ Fix

            return Created();
        }

        [HttpGet("Details/{productId}")]
        public async Task<IActionResult> Details(int productId)
        {
            var product = await _productRepository.GetOneAsync(
                e => e.ProductId == productId,
                includes: [e => e.Category, e => e.Brand, e => e.Reviews]  // ✅ زودنا includes
            );

            if (product is null)
                return NotFound();

            // ✅ جيب اليوزرز بتاعين الـ reviews
            var userIds = product.Reviews.Select(r => r.UserId).Distinct().ToList();
            var users = await _userManager.Users
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();

            var response = new ProductDetailsResponse
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Description = product.Description,
                Status = product.Status,
                MainImg = product.MainImg,
                Price = product.Price,
                PriceAfterDiscount = product.PriceAfterDiscount,
                Discount = product.Discount,
                Quantity = product.Quantity,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name ?? string.Empty,
                BrandId = product.BrandId,
                BrandName = product.Brand?.Name ?? string.Empty,
                Rate = product.Rate,
                ReviewsCount = product.Reviews.Count,
                Reviews = product.Reviews.Select(r =>
                {
                    var user = users.FirstOrDefault(u => u.Id == r.UserId);
                    return new ReviewResponse
                    {
                        UserId = r.UserId,
                        UserName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown",
                        RatingValue = r.RatingValue,
                        CreatedAt = r.CreatedAt
                    };
                }).ToList()
            };

            return Ok(response);
        }

        [HttpPut("Edit/{id}")]
        public async Task<IActionResult> Edit(int id, [FromForm] UpdateProductRequest updateProductRequest)
        {
            var productInDB = await _productRepository.GetOneAsync(e => e.ProductId == id, tracked: false);

            if (productInDB is null)
                return NotFound();

            var product = updateProductRequest.Adapt<Product>();
            product.ProductId = id; // ✅ Fix

            if (updateProductRequest.MainImg is not null && updateProductRequest.MainImg.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(updateProductRequest.MainImg.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", fileName);

                using (var stream = System.IO.File.Create(filePath))
                {
                    await updateProductRequest.MainImg.CopyToAsync(stream);
                }

                var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", productInDB.MainImg);
                if (System.IO.File.Exists(oldFilePath))
                    System.IO.File.Delete(oldFilePath);

                product.MainImg = fileName;
            }
            else
            {
                product.MainImg = productInDB.MainImg;
            }

            _productRepository.Edit(product);
            await _productRepository.CommitAsync(); // ✅ Fix

            return NoContent();
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var product = await _productRepository.GetOneAsync(e => e.ProductId == id);

            if (product is null)
                return NotFound();

            var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", product.MainImg);
            if (System.IO.File.Exists(oldFilePath))
                System.IO.File.Delete(oldFilePath);

            _productRepository.Delete(product);
            await _productRepository.CommitAsync(); // ✅ Fix

            return NoContent();
        }
    }
}