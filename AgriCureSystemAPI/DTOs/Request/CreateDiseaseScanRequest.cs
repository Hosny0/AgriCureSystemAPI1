using System.ComponentModel.DataAnnotations;

namespace AgriCureSystemAPI.DTOs.Request
{
    public class CreateDiseaseScanRequest
    {
        [Required]
        public string PlantName { get; set; } = string.Empty;  

        [Required]
        public IFormFile Image { get; set; } = null!;

        public bool IsValidImage()
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(Image.FileName).ToLower();
            var maxSize = 5 * 1024 * 1024;
            return allowedExtensions.Contains(extension) && Image.Length <= maxSize;
        }
    }
}