using AgriCureSystemAPI.Models;
using AgriCureSystemAPI.Repositories.IRepositories;
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
    public class CheckoutsController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly ICartRepository _cartRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IProductRepository _productRepository;

        public CheckoutsController(IOrderRepository orderRepository, IOrderItemRepository orderItemRepository, ICartRepository cartRepository, UserManager<ApplicationUser> userManager, IProductRepository productRepository)
        {
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _cartRepository = cartRepository;
            _userManager = userManager;
            _productRepository = productRepository;
        }
        [HttpPost("Success/{orderId}")]
        public async Task<IActionResult> Success(int orderId)
        {
            var order = await _orderRepository.GetOneAsync(e => e.Id == orderId);

            if (order is null)
                return NotFound();

            // update order status
            order.OrderStatus = OrderStatus.processing;
            var service = new SessionService();
            var session = service.Get(order.SessionId);
            order.TransactionId = session.PaymentIntentId;

            // cart => order item
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
            var carts = await _cartRepository.GetAsync(e => e.ApplicationUserId == user.Id, includes: [e => e.Product]);

            var orderItems = carts.Select(e => new OrderItem()
            {
                OrderId = orderId,
                ProductId = e.ProductId,
                Price = (double)e.Product.Price,
                Quantity = e.Count
            }).ToList();

            await _orderItemRepository.CreateRangeAsync(orderItems);

            foreach (var item in carts)
            {
                // decrease quantity
                item.Product.Quantity -= item.Count;

                // delete old cart
                _cartRepository.Delete(item);
            }

            await _orderRepository.CommitAsync();
            await _orderItemRepository.CommitAsync();
            await _cartRepository.CommitAsync();
            await _productRepository.CommitAsync();

            return Created();
        }

        [HttpPost("Cancel/{orderId}")]
        public async Task<IActionResult> Cancel(int orderId)
        {
            var order = await _orderRepository.GetOneAsync(e => e.Id == orderId);

            if (order is null)
                return NotFound();

            // update order status
            order.OrderStatus = OrderStatus.canceled;

            await _orderRepository.CommitAsync();

            return NoContent();
        }


    }
}
