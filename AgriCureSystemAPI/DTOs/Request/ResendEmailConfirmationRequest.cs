using System.ComponentModel.DataAnnotations;

namespace AgriCureSystemAPI.DTOs.Request
{
    public class ResendEmailConfirmationRequest
    {
        [Required]
        public string EmailORUserName { get; set; } = string.Empty;
    }
}
