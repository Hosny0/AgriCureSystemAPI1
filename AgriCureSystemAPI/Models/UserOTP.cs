using System.ComponentModel.DataAnnotations;

namespace AgriCureSystemAPI.Models
{
    public class UserOTP
    {
        public int Id { get; set; }
        [Required]
        public string Code { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public bool Status { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string ApplicationUserId { get; set; } = string.Empty;
        public ApplicationUser ApplicationUser { get; set; } = null!;
    }
}
