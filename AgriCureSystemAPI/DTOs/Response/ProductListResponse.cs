namespace AgriCureSystemAPI.DTOs.Response
{
    public class ProductListResponse
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; } 
        public string MainImg { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal PriceAfterDiscount { get; set; }
        public double Discount { get; set; }
        public double Rate { get; set; }
        public int ReviewsCount { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string BrandName { get; set; } = string.Empty;
    }
}
