using System.ComponentModel.DataAnnotations;

namespace AgriCureSystemAPI.Models
{
    public class Review
    {
        
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public string UserId { get; set; } = string.Empty;

        [Range(1, 5)]
        public int RatingValue { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

