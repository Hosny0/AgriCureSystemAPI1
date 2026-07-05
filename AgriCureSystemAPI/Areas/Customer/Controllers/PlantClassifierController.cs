using AgriCureSystemAPI.DTOs.Request;
using AgriCureSystemAPI.DTOs.Response;
using AgriCureSystemAPI.Models;
using AgriCureSystemAPI.Repositories.IRepositories;
using AgriCureSystemAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AgriCureSystemAPI.Areas.Customer.Controllers
{
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Area("Customer")]
    [Authorize]
    public class PlantClassifierController : ControllerBase
    {
        private readonly IPlantClassifierService _plantClassifierService;
        private readonly IAiService _aiService;
        private readonly IDiseaseScanRepository _diseaseScanRepo;
        private readonly IWebHostEnvironment _env;

        public PlantClassifierController(
            IPlantClassifierService plantClassifierService,
            IAiService aiService,
            IDiseaseScanRepository diseaseScanRepo,
            IWebHostEnvironment env)
        {
            _plantClassifierService = plantClassifierService;
            _aiService = aiService;
            _diseaseScanRepo = diseaseScanRepo;
            _env = env;
        }

        [HttpPost("Classify")]
        public async Task<IActionResult> Classify([FromForm] CreatePlantClassifyRequest request)
        {
            if (request.Image is null || request.Image.Length == 0)
                return BadRequest("Image is required.");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(request.Image.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
                return BadRequest("Image must be jpg/jpeg/png.");

            if (request.Image.Length > 5 * 1024 * 1024)
                return BadRequest("Image must be less than 5MB.");

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 1️⃣ حوّل الصورة لـ byte array
            using var memoryStream = new MemoryStream();
            await request.Image.CopyToAsync(memoryStream);
            var imageBytes = memoryStream.ToArray();

            // 2️⃣ Plant Classifier — اعرف اسم النبات
            var plantResult = await _plantClassifierService.ClassifyPlantAsync(imageBytes, request.Image.FileName);
            if (plantResult is null || !plantResult.IsValidPlant)
                return BadRequest("No valid plant detected in the image.");

            // 3️⃣ Disease AI — شخّص المرض
            var imageFile = new FormFile(
                new MemoryStream(imageBytes), 0, imageBytes.Length,
                "file", request.Image.FileName
            );
            var aiResult = await _aiService.PredictDiseaseAsync(imageFile, plantResult.PlantNameEn);
            if (aiResult is null)
                return StatusCode(500, "Disease AI service failed.");

            // 4️⃣ احفظ الصورة في الـ wwwroot/ScanImage
            var fileName = Guid.NewGuid().ToString() + extension;
            var filePath = Path.Combine(_env.WebRootPath, "ScanImage", fileName);
            await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);

            // 5️⃣ احفظ النتيجة في الداتابيز
            var scan = new DiseaseScan
            {
                PlantName = plantResult.PlantNameEn,
                DiseaseName = aiResult.Prediction,
                ConfidenceRate = aiResult.Confidence,
                Description = aiResult.Details.Description,
                Symptoms = aiResult.Details.Symptoms,
                Treatment = aiResult.Details.Treatment,
                ImageUrl = fileName,
                ScanDate = DateTime.UtcNow,
                UserId = currentUserId!
            };

            await _diseaseScanRepo.CreateAsync(scan);
            var saved = await _diseaseScanRepo.CommitAsync();
            if (!saved)
                return StatusCode(500, "Failed to save scan result.");

            // 6️⃣ رجّع النتيجة الكاملة على طول
            return Ok(new DiseaseScanResponse
            {
                Id = scan.Id,
                PlantName = scan.PlantName,
                DiseaseName = scan.DiseaseName,
                ConfidenceRate = scan.ConfidenceRate,
                Description = scan.Description,
                Symptoms = scan.Symptoms,
                Treatment = scan.Treatment,
                ImageUrl = scan.ImageUrl,
                ScanDate = scan.ScanDate
            });
        }
    }
}