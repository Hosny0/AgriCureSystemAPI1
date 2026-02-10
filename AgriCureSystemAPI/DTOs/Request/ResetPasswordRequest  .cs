using System.ComponentModel.DataAnnotations;

namespace AgriCureSystemAPI.DTOs.Request
{
    public class ResetPasswordRequest
    {
        [Required]
        public string Code { get; set; } = string.Empty;
        public string UserId { get; set; }
    }
}
