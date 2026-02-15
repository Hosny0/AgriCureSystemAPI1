using System.ComponentModel.DataAnnotations;

namespace AgriCureSystemAPI.DTOs.Requests
{
    public class UpdateProductRequest
    {
        [Required]
        [MinLength(3)]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool Status { get; set; }
        public IFormFile? MainImg { get; set; } 
        [Range(0, 1_000_000)]
        public decimal Price { get; set; }
        [Range(0, 50_000)]
        public int Quantity { get; set; }
        [Range(0, 100)]
        public double Discount { get; set; }
        public int CategoryId { get; set; }
        public int BrandId { get; set; }
    }
}
