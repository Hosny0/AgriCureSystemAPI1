using Microsoft.EntityFrameworkCore;

namespace AgriCureSystemAPI.Models
{
    [PrimaryKey(nameof(OrderId), nameof(ProductId))]
    public class OrderItem
    {
        public int OrderId { get; set; }
        public Order Order { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public double Price { get; set; }
        public double Quantity { get; set; }
        
    }
}
