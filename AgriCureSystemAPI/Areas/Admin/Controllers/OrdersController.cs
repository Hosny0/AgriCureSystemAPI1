using AgriCureSystemAPI.Models;
using AgriCureSystemAPI.Repositories.IRepositories;
using AgriCureSystemAPI.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AgriCureSystemAPI.Areas.Admin.Controllers
{
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Area("Admin")]
    [Authorize(Roles = $"{SD.SuperAdmin},{SD.Admin}")]

    public class OrdersController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;

        public OrdersController(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var orders = await _orderRepository.GetAsync();

            return Ok(orders);
        }

        [HttpGet("Get/{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var order = await _orderRepository.GetOneAsync(e => e.Id == id, includes: [equals => equals.ApplicationUser]);

            if (order is null)
                return NotFound();

            return Ok(order);
        }

        [HttpPatch("Shipped/{id}")]
        public async Task<IActionResult> Shipped(int id)
        {
            var order = await _orderRepository.GetOneAsync(e => e.Id == id, includes: [equals => equals.ApplicationUser]);

            if (order is null)
                return NotFound();

            order.OrderStatus = OrderStatus.shipped;
            await _orderRepository.CommitAsync();

            return Ok("Update successfully");
        }

        [HttpPatch("Complete/{id}")]
        public async Task<IActionResult> Complete(int id)
        {
            var order = await _orderRepository.GetOneAsync(e => e.Id == id, includes: [equals => equals.ApplicationUser]);

            if (order is null)
                return NotFound();

            order.OrderStatus = OrderStatus.completed;
            await _orderRepository.CommitAsync();

            return Ok("Update successfully");
        }

        [HttpPatch("Canceled/{id}")]
        public async Task<IActionResult> Canceled(int id)
        {
            var order = await _orderRepository.GetOneAsync(e => e.Id == id, includes: [equals => equals.ApplicationUser]);

            if (order is null)
                return NotFound();

            order.OrderStatus = OrderStatus.canceled;
            await _orderRepository.CommitAsync();

            return Ok("Update successfully");
        }
    }
}
