using AgriCureSystemAPI.DTOs.Response;
using AgriCureSystemAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriCureSystemAPI.Areas.Customer.Controllers
{
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Area("Customer")]
    [Authorize]
    public class PlantClassifierController : ControllerBase
    {
        private readonly IPlantClassifierService _plantClassifierService;

        public PlantClassifierController(IPlantClassifierService plantClassifierService)
        {
            _plantClassifierService = plantClassifierService;
        }

        [HttpPost("Classify")]
        public async Task<IActionResult> Classify([FromForm] IFormFile image)
        {
            if (image is null || image.Length == 0)
                return BadRequest("Image is required.");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(image.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
                return BadRequest("Image must be jpg/jpeg/png.");

            if (image.Length > 5 * 1024 * 1024)
                return BadRequest("Image must be less than 5MB.");

            using var memoryStream = new MemoryStream();
            await image.CopyToAsync(memoryStream);
            var imageBytes = memoryStream.ToArray();

            var result = await _plantClassifierService.ClassifyPlantAsync(imageBytes, image.FileName);

            if (result is null)
                return StatusCode(500, "Plant Classifier service failed.");

            if (!result.IsValidPlant)
                return BadRequest("No valid plant detected in the image.");

            return Ok(new
            {
                PlantNameEn = result.PlantNameEn,
                PlantNameAr = result.PlantNameAr,
                Confidence = result.Confidence,
                IsValidPlant = result.IsValidPlant
            });
        }
    }
}