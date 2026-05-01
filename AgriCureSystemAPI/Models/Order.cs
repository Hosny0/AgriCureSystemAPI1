namespace AgriCureSystemAPI.Models
{
    public enum OrderStatus
    {
        pending,
        processing,
        shipped,
        inWay,
        completed,
        canceled,
        refunded
    }

    public enum PaymentMethod
    {
        Visa,
        CoD
    }

    public class Order
    {
        public int Id { get; set; }
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public DateTime DateTime { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public double TotalPrice { get; set; }

        public string? Carrier { get; set; }
        public string? CarrierId { get; set; }

        public string? TransactionId { get; set; }
        public string? SessionId { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }
    }
}
