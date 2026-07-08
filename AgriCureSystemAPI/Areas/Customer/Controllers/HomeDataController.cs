using AgriCureSystemAPI.Data;
using AgriCureSystemAPI.DTOs.Response;
using AgriCureSystemAPI.Models;
using AgriCureSystemAPI.Utility;
using ECommerce.API.DTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgriCureSystemAPI.Areas.Customer.Controllers
{
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Area("Customer")]
    [Authorize(Roles = $"{SD.SuperAdmin},{SD.Admin},{SD.Customer},{SD.Employee}")]
    public class HomeDataController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeDataController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("Index")]
        public IActionResult Get([FromQuery] ProductFilterRequest? productFilterRequest, int page = 1)
        {
            productFilterRequest ??= new ProductFilterRequest();

            const double discount = 50;
            IQueryable<Product> products = _context.Products;

            products = products
                .Include(e => e.Category)
                .Include(e => e.Brand)       
                .Include(e => e.Reviews);

            var allCategories = _context.Categories
                .Select(c => new { Id = c.Id, Name = c.Name })
                .ToList();

            // Filters
            if (productFilterRequest.ProductName is not null)
                products = products.Where(e => e.Name.Contains(productFilterRequest.ProductName));

            if (productFilterRequest.MinPrice is not null)
                products = products.Where(e => e.Price - e.Price * ((decimal)e.Discount / 100) >= (decimal)productFilterRequest.MinPrice);

            if (productFilterRequest.MaxPrice is not null)
                products = products.Where(e => e.Price - e.Price * ((decimal)e.Discount / 100) <= (decimal)productFilterRequest.MaxPrice);

            if (productFilterRequest.CategoryId > 0)
                products = products.Where(e => e.CategoryId == productFilterRequest.CategoryId);
            else if (!string.IsNullOrEmpty(productFilterRequest.CategoryName))
                products = products.Where(e => e.Category.Name.Contains(productFilterRequest.CategoryName));

            if (productFilterRequest.IsHot)
                products = products.Where(e => e.Discount > discount);

            // Pagination
            if (page < 1) page = 1;

            var totalCount = products.Count();

            var pagination = new
            {
                TotalNumberOfPage = Math.Ceiling(totalCount / 8.0),
                CurrentPage = page
            };

           
            var productList = products
                .Skip((page - 1) * 8)
                .Take(8)
                .ToList()
                .Select(p => new ProductListResponse
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Description = p.Description,
                    Quantity = p.Quantity, // ✅ زودها
                    MainImg = p.MainImg,
                    Price = p.Price,
                    PriceAfterDiscount = p.PriceAfterDiscount,
                    Discount = p.Discount,
                    Rate = p.Rate,
                    ReviewsCount = p.Reviews.Count,
                    CategoryName = p.Category?.Name ?? string.Empty,
                    BrandName = p.Brand?.Name ?? string.Empty
                }).ToList();

            var returned = new
            {
                ProductName = productFilterRequest.ProductName,
                MinPrice = productFilterRequest.MinPrice,
                MaxPrice = productFilterRequest.MaxPrice,
                CategoryId = productFilterRequest.CategoryId,
                CategoryName = productFilterRequest.CategoryName,
                IsHot = productFilterRequest.IsHot,
                Products = productList
            };

            return Ok(new
            {
                CategoriesList = allCategories,
                pagination,
                returned
            });
        }

        [HttpGet("Details/{id}")]
        public async Task<IActionResult> GetOne([FromRoute] int id)
        {
            var product = _context.Products
                .Include(e => e.Category)
                .Include(e => e.Brand)
                .Include(e => e.Reviews)
                .FirstOrDefault(e => e.ProductId == id);

            if (product is null)
                return NotFound();

            var userIds = product.Reviews.Select(r => r.UserId).Distinct().ToList();
            var users = await _userManager.Users
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();

            var productResponse = new ProductDetailsResponse
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

            var relatedProducts = _context.Products
                .Include(e => e.Category)
                .Include(e => e.Brand)
                .Include(e => e.Reviews)
                .Where(e => e.CategoryId == product.CategoryId && e.ProductId != product.ProductId)
                .Take(4)
                .ToList()
                .Select(p => new ProductListResponse
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Description = p.Description,
                    Quantity = p.Quantity,  // ✅ زود السطر ده

                    MainImg = p.MainImg,
                    Price = p.Price,
                    PriceAfterDiscount = p.PriceAfterDiscount,
                    Discount = p.Discount,
                    Rate = p.Rate,
                    ReviewsCount = p.Reviews.Count,
                    CategoryName = p.Category?.Name ?? string.Empty,
                    BrandName = p.Brand?.Name ?? string.Empty
                }).ToList();

            product.Traffic++;
            _context.SaveChanges();

            return Ok(new
            {
                Product = productResponse,
                RelatedProducts = relatedProducts
            });
        }
    }
    
}