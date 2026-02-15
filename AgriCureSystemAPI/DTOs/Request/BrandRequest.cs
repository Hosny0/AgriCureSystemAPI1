using System.ComponentModel.DataAnnotations;

namespace AgriCureSystemAPI.DTOs.Requests
{
    public class BrandRequest
    {
        [Required]
        [MinLength(3)]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        //[CustomLengthValidation(20)]
        public string? Description { get; set; }
        public bool Status { get; set; }
    }
}
