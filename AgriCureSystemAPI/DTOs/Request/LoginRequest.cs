using System.ComponentModel.DataAnnotations;

namespace AgriCureSystemAPI.DTOs.Request
{
    public class LoginRequest
    {
        [Required]
        public string EmailORUserName { get; set; } = string.Empty;
        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }
}
