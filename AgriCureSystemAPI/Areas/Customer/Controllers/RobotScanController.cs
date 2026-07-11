using AgriCureSystemAPI.DTOs.Response;
using AgriCureSystemAPI.Models;
using AgriCureSystemAPI.Repositories.IRepositories;
using AgriCureSystemAPI.Services;
using AgriCureSystemAPI.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AgriCureSystemAPI.Areas.Customer.Controllers
{
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Area("Customer")]
    [Authorize]
    public class RobotScanController : ControllerBase
    {
        private readonly IRobotService _robotService;
        private readonly IPlantClassifierService _plantClassifierService;
        private readonly IAiService _aiService;
        private readonly IDiseaseScanRepository _diseaseScanRepo;

        public RobotScanController(
            IRobotService robotService,
            IPlantClassifierService plantClassifierService,
            IAiService aiService,
            IDiseaseScanRepository diseaseScanRepo)
        {
            _robotService = robotService;
            _plantClassifierService = plantClassifierService;
            _aiService = aiService;
            _diseaseScanRepo = diseaseScanRepo;
        }

        private async Task<DiseaseScanResponse> ProcessSingleScan(
            RobotScanItem robotScan,
            string currentUserId)
        {
            try
            {
                // 1️⃣ حوّل الـ base64 لـ byte array
                var imageBytes = Convert.FromBase64String(robotScan.ImageBase64);
                var fileName = $"{robotScan.Direction}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.jpg";

                // 2️⃣ Plant Classifier — هل ده نبات؟
                var plantResult = await _plantClassifierService.ClassifyPlantAsync(imageBytes, fileName);

                // ✅ تحقق من الـ confidence
                double confidence = 0;
                if (plantResult?.Confidence != null)
                {
                    var confidenceStr = plantResult.Confidence.Replace("%", "").Trim();
                    double.TryParse(confidenceStr,
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out confidence);
                }

                // ✅ لو مش نبات
                if (plantResult is null || !plantResult.IsValidPlant || confidence < 90)
                {
                    return new DiseaseScanResponse
                    {
                        Description = "No valid plant detected in the image."
                    };
                }

                // 3️⃣ Disease AI — بعت الصورة + اسم النبات
                var imageFile = new FormFile(
                    new MemoryStream(imageBytes), 0, imageBytes.Length, "file", fileName
                );
                var aiResult = await _aiService.PredictDiseaseAsync(imageFile, plantResult.PlantNameEn);

                if (aiResult is null)
                {
                    return new DiseaseScanResponse
                    {
                        Description = "No valid plant detected in the image."
                    };
                }

                // 4️⃣ احفظ في الداتابيز
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
                    UserId = currentUserId
                };

                await _diseaseScanRepo.CreateAsync(scan);
                await _diseaseScanRepo.CommitAsync();

                return new DiseaseScanResponse
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
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
                return new DiseaseScanResponse
                {
                    Description = "No valid plant detected in the image."
                };
            }
        }

        [HttpGet("ScanLatest")]
        public async Task<IActionResult> ScanLatest()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var latest = await _robotService.GetLatestScansAsync();
            if (latest is null)
                return NotFound("No latest scan available from robot.");

            var results = new List<object>();

            foreach (var robotScan in new[] { latest.Left, latest.Right }.Where(s => s is not null))
            {
                var result = await ProcessSingleScan(robotScan!, currentUserId!);

                // ✅ لو مش نبات ارجع رسالة بسيطة
                if (result.Description == "No valid plant detected in the image.")
                {
                    results.Add(new { message = "No valid plant detected in the image." });
                }
                else
                {
                    results.Add(result);
                }
            }

            if (results.Count == 0)
                return NotFound("No scans available from robot.");

            return Ok(new { Total = results.Count, Results = results });
        }

        [HttpGet("Stats")]
        public async Task<IActionResult> Stats()
        {
            var stats = await _robotService.GetStatsAsync();
            if (stats is null)
                return StatusCode(500, "Failed to get robot stats.");
            return Ok(stats);
        }

        [HttpGet("Status")]
        public async Task<IActionResult> Status()
        {
            var status = await _robotService.GetRobotStatusAsync();
            if (status is null)
                return StatusCode(500, "Failed to get robot status.");
            return Ok(status);
        }

        [HttpPost("Start")]
        [Authorize(Roles = $"{SD.SuperAdmin},{SD.Admin}")]
        public async Task<IActionResult> Start()
        {
            await _robotService.StartRobotAsync();
            return Ok(new { message = "Robot started successfully." });
        }

        [HttpPost("Stop")]
        [Authorize(Roles = $"{SD.SuperAdmin},{SD.Admin}")]
        public async Task<IActionResult> Stop()
        {
            await _robotService.StopRobotAsync();
            return Ok(new { message = "Robot stopped successfully." });
        }
    }
}