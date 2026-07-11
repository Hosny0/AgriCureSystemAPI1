using AgriCureSystemAPI.Models;
using AgriCureSystemAPI.Repositories.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;

namespace AgriCureSystemAPI.Areas.Customer.Controllers
{
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Area("Customer")]
    [Authorize]
    public class CartsController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly IOrderRepository _orderRepository;

        public CartsController(UserManager<ApplicationUser> userManager, ICartRepository cartRepository, IProductRepository productRepository, IOrderRepository orderRepository)
        {
            _userManager = userManager;
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _orderRepository = orderRepository;
        }
        [HttpPost("AddToCart")]
        public async Task<IActionResult> AddToCart(int productId, int count)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null)
            {
                string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (userId is null)
                    return NotFound();

                user = await _userManager.FindByIdAsync(userId);
            }


            var product = await _productRepository.GetOneAsync(e => e.ProductId == productId);
            if (product is null || product.Quantity < count)
            {
                return BadRequest("Invalid Count");
            }

            var cartInDb = await _cartRepository.GetOneAsync(e => e.ProductId == productId && e.ApplicationUserId == user.Id);

            if (cartInDb is not null)
                cartInDb.Count += count;
            else
            {
                await _cartRepository.CreateAsync(new()
                {
                    ApplicationUserId = user.Id,
                    Count = count,
                    ProductId = productId
                });
            }

            await _cartRepository.CommitAsync();

            return Ok("Add Product to cart Successfully");
        }

        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null)
            {
                string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId is null) return NotFound();
                user = await _userManager.FindByIdAsync(userId);
            }

            var carts = await _cartRepository.GetAsync(e => e.ApplicationUserId == user.Id, includes: [e => e.Product]);

            return Ok(new
            {
                Carts = carts,
                TotalPrice = carts.Sum(e => e.Product.PriceAfterDiscount * e.Count)
            });
        }

        [HttpPatch("IncrementCount/{productId}")]
        public async Task<IActionResult> IncrementCount([FromRoute] int productId)
        {

            var user = await _userManager.GetUserAsync(User);

            if (user is null)
            {
                string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (userId is null)
                    return NotFound();

                user = await _userManager.FindByIdAsync(userId);
            }

            if (user is null)
            {
                return NotFound();
            }

            var cartInDb = await _cartRepository.GetOneAsync(e => e.ProductId == productId && e.ApplicationUserId == user.Id);

            if (cartInDb is null)
                return NotFound();

            cartInDb.Count++;
            await _cartRepository.CommitAsync();

            return NoContent();
        }

        [HttpPatch("DecrementCount/{productId}")]
        public async Task<IActionResult> DecrementCount([FromRoute] int productId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null)
            {
                string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (userId is null)
                    return NotFound();

                user = await _userManager.FindByIdAsync(userId);
            }

            if (user is null)
            {
                return NotFound();
            }

            var cartInDb = await _cartRepository.GetOneAsync(e => e.ProductId == productId && e.ApplicationUserId == user.Id);

            if (cartInDb is null)
                return NotFound();

            if (cartInDb.Count > 1)
            {
                cartInDb.Count--;
                await _cartRepository.CommitAsync();
            }

            return NoContent();
        }

        [HttpPatch("DeleteProduct/{productId}")]
        public async Task<IActionResult> DeleteProduct(int productId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null)
            {
                string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (userId is null)
                    return NotFound();

                user = await _userManager.FindByIdAsync(userId);
            }

            if (user is null)
            {
                return NotFound();
            }

            var cartInDb = await _cartRepository.GetOneAsync(e => e.ProductId == productId && e.ApplicationUserId == user.Id);

            if (cartInDb is null)
                return NotFound();

            _cartRepository.Delete(cartInDb);
            await _cartRepository.CommitAsync();

            return NoContent();
        }

        [HttpPost("Pay")]
        public async Task<IActionResult> Pay()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null)
            {
                string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId is null) return NotFound();
                user = await _userManager.FindByIdAsync(userId);
            }

            var carts = await _cartRepository.GetAsync(e => e.ApplicationUserId == user.Id, includes: [e => e.Product]);

            if (!carts.Any()) return BadRequest("Cart is empty");

            await _orderRepository.CreateAsync(new()
            {
                ApplicationUserId = user.Id,
                DateTime = DateTime.UtcNow,
                OrderStatus = OrderStatus.pending,
                PaymentMethod = PaymentMethod.Visa,
                TotalPrice = (double)carts.Sum(e => e.Product.PriceAfterDiscount * e.Count),
            });
            await _orderRepository.CommitAsync();

            var order = (await _orderRepository.GetAsync(e => e.ApplicationUserId == user.Id))
                        .OrderBy(e => e.Id)
                        .LastOrDefault();

            if (order is null) return NotFound();

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = $"{Request.Scheme}://{Request.Host}/Customer/Checkout/Success?orderId={order.Id}",
                CancelUrl = $"{Request.Scheme}://{Request.Host}/Customer/Checkout/Cancel?orderId={order.Id}",
            };

            foreach (var item in carts)
            {
                options.LineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "egp",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Name,
                            Description = item.Product.Description
                        },
                        UnitAmount = (long)(item.Product.PriceAfterDiscount * 100),
                    },
                    Quantity = item.Count,
                });
            }

            var service = new SessionService();
            var session = service.Create(options);
            order.SessionId = session.Id;
            await _orderRepository.CommitAsync();

            return Ok(new { url = session.Url });
        }

    }
}
