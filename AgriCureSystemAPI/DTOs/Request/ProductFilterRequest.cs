namespace ECommerce.API.DTOs.Requests
{
    public class ProductFilterRequest
    {
        public string? ProductName { get; set; }
        public double? MinPrice { get; set; }
        public double? MaxPrice { get; set; }
        public int CategoryId { get; set; }
        public bool IsHot { get; set; }
    }
}
