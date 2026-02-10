using AgriCureSystemAPI.Models;

namespace AgriCureSystemAPI.Repositories.IRepositories
{
    public interface IOrderItemRepository : IRepository<OrderItem>
    {
        Task CreateRangeAsync(List<OrderItem> orderItems);
    }
}
