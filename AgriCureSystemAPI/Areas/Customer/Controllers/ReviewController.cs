using AgriCureSystemAPI.DTOs.Response;
using AgriCureSystemAPI.Models;
using AgriCureSystemAPI.Repositories.IRepositories;
using AgriCureSystemAPI.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AgriCureSystemAPI.Areas.Customer.Controllers
{
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Area("Customer")]
    [Authorize(Roles = $"{SD.SuperAdmin},{SD.Admin},{SD.Customer},{SD.Employee}")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewRepository _reviewRepo;
        private readonly IProductRepository _productRepo; 

        public ReviewController(IReviewRepository reviewRepo, IProductRepository productRepo)
        {
            _reviewRepo = reviewRepo;
            _productRepo = productRepo;
        }

        [HttpPost("AddRating")]
        public async Task<IActionResult> AddRating([FromForm] int productId, [FromForm] int ratingValue)
        {
            if (ratingValue < 1 || ratingValue > 5)
                return BadRequest("The rating should be between 1 and 5");

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var existingReview = await _reviewRepo.GetUserReviewAsync(productId, currentUserId);

            if (existingReview != null)
            {
                existingReview.RatingValue = ratingValue;
                existingReview.CreatedAt = DateTime.UtcNow;
                _reviewRepo.Edit(existingReview);
            }
            else
            {
                var newReview = new Review
                {
                    ProductId = productId,
                    UserId = currentUserId,
                    RatingValue = ratingValue
                };
                await _reviewRepo.CreateAsync(newReview);
            }

            await _reviewRepo.CommitAsync();

            var product = await _productRepo.GetOneAsync(
                e => e.ProductId == productId,
                includes: [e => e.Reviews]
            );

            var response = new AddRatingResponse
            {
                Message = "The rating was successfully saved",
                ProductId = productId,
                NewRate = product?.Rate ?? 0,
                ReviewsCount = product?.Reviews.Count ?? 0
            };

            return Ok(response);
        }
    }
}