using Microsoft.EntityFrameworkCore;

namespace AgriCureSystemAPI.Models
{
    [PrimaryKey(nameof(ApplicationUserId), nameof(ProductId))]
    public class Cart
    {
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int Count { get; set; }
    }
}
