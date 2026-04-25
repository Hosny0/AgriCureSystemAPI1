using AgriCureSystemAPI.Data;
using AgriCureSystemAPI.Models;
using AgriCureSystemAPI.Repositories.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AgriCureSystemAPI.Areas.Customer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewRepository _reviewRepo;

        public ReviewController(IReviewRepository reviewRepo)
        {
            _reviewRepo = reviewRepo;
        }

        [HttpPost("AddRating")]
        public async Task<IActionResult> AddRating([FromForm] int productId, [FromForm] int ratingValue)
        {
            if (ratingValue < 1 || ratingValue > 5)
                return BadRequest("The rating should be between 1 and 5");

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // استخدمنا الدالة المخصصة اللي لسه كاتبينها
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

         
            return Ok("The rating was successfully saved");
        }
    }
}
