using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace AgriCureSystemAPI.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        [Required]
        [MinLength(3)]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool Status { get; set; }
        [ValidateNever]
        public string MainImg { get; set; } = string.Empty;
        [Range(0, 1_000_000)]
        public decimal Price { get; set; }
        [Range(0, 50_000)]
        public int Quantity { get; set; }
        [Range(0, 5)]
        public double Rate { get; set; }
        [Range(0, 100)]
        public double Discount { get; set; }
        public int Traffic { get; set; }
        public int CategoryId { get; set; }
        [ValidateNever]
        public Category Category { get; set; } = null!;
        public int BrandId { get; set; }
        [ValidateNever]
        public Brand Brand { get; set; } = null!;
    }
}
