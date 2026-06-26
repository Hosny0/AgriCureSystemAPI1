namespace AgriCureSystemAPI.DTOs.Response
{
    public class ProductDetailsResponse
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool Status { get; set; }
        public string MainImg { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal PriceAfterDiscount { get; set; }
        public double Discount { get; set; }
        public int Quantity { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int BrandId { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public double Rate { get; set; }
        public int ReviewsCount { get; set; }
        public List<ReviewResponse> Reviews { get; set; } = new();

    }
    } 

