using System.ComponentModel.DataAnnotations;

namespace AgriCureSystemAPI.DTOs.Response
{
    public class ApplicationUserResponse
    {
        public string ApplicationUserId { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }

        [DataType(DataType.Password)]
        public string? OldPassword { get; set; }
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }
        [DataType(DataType.Password)]
        public string? ConfirmNewPassword { get; set; }
    }
}
