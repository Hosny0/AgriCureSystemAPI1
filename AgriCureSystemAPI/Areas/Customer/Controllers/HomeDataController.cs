using AgriCureSystemAPI.Data;
using AgriCureSystemAPI.Models;
using AgriCureSystemAPI.Repositories.IRepositories;
using ECommerce.API.DTOs.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgriCureSystemAPI.Areas.Customer.Controllers
{
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Area("Customer")]
    public class HomeDataController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly ApplicationDbContext _context;

        public HomeDataController(IProductRepository productRepository, ApplicationDbContext context)
        {
            _productRepository = productRepository;
            _context = context;
        }
        [HttpGet("Index")]
        public IActionResult Get([FromBody] ProductFilterRequest? productFilterRequest, int page = 1)
        {
            const double discount = 50;
            IQueryable<Product> products = _context.Products;
            var categories = _context.Categories;

            // Join
            products = products.Include(e => e.Category);

            // Filter
            if (productFilterRequest.ProductName is not null)
            {
                products = products.Where(e => e.Name.Contains(productFilterRequest.ProductName));
            }

            if (productFilterRequest.MinPrice is not null)
            {
                products = products.Where(e => e.Price - e.Price * ((decimal)e.Discount / 100) >= (decimal)productFilterRequest.MinPrice);
            }

            if (productFilterRequest.MaxPrice is not null)
            {
                products = products.Where(e => e.Price - e.Price * ((decimal)e.Discount / 100) <= (decimal)productFilterRequest.MaxPrice);
            }

            if (productFilterRequest.CategoryId > 0 && productFilterRequest.CategoryId <= categories.Count())
            {
                products = products.Where(e => e.CategoryId == productFilterRequest.CategoryId);
            }

            if (productFilterRequest.IsHot)
            {
                products = products.Where(e => e.Discount > discount);
            }



            // Pagination
            if (page < 0)
                page = 1;

            var pagination = new
            {
                TotalNumberOfPage = Math.Ceiling(products.Count() / 8.0),
                CurrentPage = page
            };

            // Data
            var returned = new
            {
                ProductName = productFilterRequest.ProductName,
                MinPrice = productFilterRequest.MinPrice,
                MaxPrice = productFilterRequest.MaxPrice,
                CategoryId = productFilterRequest.CategoryId,
                IsHot = productFilterRequest.IsHot,
                products = products.Skip((page - 1) * 8).Take(8).ToList()
            };

            return Ok(new
            {
                pagination,
                returned
            });
        }
        [HttpGet("Details/{id}")]
        public IActionResult GetOne([FromRoute] int id)
        {
            var product = _context.Products.Include(e => e.Category).Include(e => e.Brand).FirstOrDefault(e => e.ProductId == id);

            if (product is not null)
            {
                var relatedProducts = _context.Products.Include(e => e.Category).Where(e => e.CategoryId == product.CategoryId && e.ProductId != product.ProductId).Skip(0).Take(4);

                var topProduct = _context.Products.Include(e => e.Category).Where(e => e.ProductId != product.ProductId).OrderByDescending(e => e.Traffic).Skip(0).Take(4);

                var similarProduct = _context.Products.Include(e => e.Category).Where(e => e.Name.Contains(product.Name) && e.ProductId != product.ProductId).Skip(0).Take(4);

                var ProductWithRelated = new
                {
                    Product = product,
                    RelatedProducts = relatedProducts.ToList(),
                    TopProduct = topProduct.ToList(),
                    SimilarProduct = similarProduct.ToList()
                };

                product.Traffic++;
                _context.SaveChanges();

                return Ok(ProductWithRelated);
            }

            return NotFound();
        }
    }
}
