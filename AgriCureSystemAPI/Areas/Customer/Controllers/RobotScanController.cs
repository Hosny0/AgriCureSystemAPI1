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
        private readonly IConfiguration _configuration;

        public RobotScanController(
            IRobotService robotService,
            IPlantClassifierService plantClassifierService,
            IAiService aiService,
            IDiseaseScanRepository diseaseScanRepo,
            IConfiguration configuration)
        {
            _robotService = robotService;
            _plantClassifierService = plantClassifierService;
            _aiService = aiService;
            _diseaseScanRepo = diseaseScanRepo;
            _configuration = configuration;
        }

        // ✅ Helper — يحلل صورة واحدة بالـ 3 APIs
        private async Task<DiseaseScanResponse?> ProcessSingleScan(
            RobotScanItem robotScan,
            HttpClient httpClient,
            string robotBaseUrl,
            string currentUserId)
        {
            try
            {
                // 1️⃣ جيب الصورة من Robot API
                var fullImageUrl = $"{robotBaseUrl}{robotScan.ImageUrl}";
                var imageBytes = await httpClient.GetByteArrayAsync(fullImageUrl);
                var fileName = $"{robotScan.ScanId}.jpg";

                // 2️⃣ Plant Classifier — اعرف اسم النبات
                var plantResult = await _plantClassifierService.ClassifyPlantAsync(imageBytes, fileName);
                if (plantResult is null || !plantResult.IsValidPlant)
                    return null;

                // 3️⃣ Disease AI — جيب التفاصيل
                var imageFile = new FormFile(
                    new MemoryStream(imageBytes), 0, imageBytes.Length, "file", fileName
                );
                var aiResult = await _aiService.PredictDiseaseAsync(imageFile, plantResult.PlantNameEn);
                if (aiResult is null)
                    return null;

                // 4️⃣ احفظ في الداتابيز
                var scan = new DiseaseScan
                {
                    PlantName = plantResult.PlantNameEn,
                    DiseaseName = aiResult.Prediction,
                    ConfidenceRate = $"{robotScan.Confidence}%",
                    Description = aiResult.Details.Description,
                    Symptoms = aiResult.Details.Symptoms,
                    Treatment = aiResult.Details.Treatment,
                    ImageUrl = fullImageUrl,
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
            catch
            {
                return null;
            }
        }

        // ✅ آخر يمين + يسار من الـ Robot
        [HttpPost("ScanLatest")]
        public async Task<IActionResult> ScanLatest()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var robotBaseUrl = _configuration["RobotApi:BaseUrl"]!;

            var latest = await _robotService.GetLatestScansAsync();
            if (latest is null)
                return NotFound("No latest scan available from robot.");

            var results = new List<DiseaseScanResponse>();
            using var httpClient = new HttpClient();

            foreach (var robotScan in new[] { latest.Left, latest.Right }.Where(s => s is not null))
            {
                var result = await ProcessSingleScan(robotScan!, httpClient, robotBaseUrl, currentUserId!);
                if (result is not null)
                    results.Add(result);
            }

            if (results.Count == 0)
                return StatusCode(500, "Failed to process robot scans.");

            return Ok(new { Total = results.Count, Results = results });
        }

        // ✅ كل صور الـ Robot
        [HttpPost("ScanAll")]
        public async Task<IActionResult> ScanAll()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var robotBaseUrl = _configuration["RobotApi:BaseUrl"]!;

            var robotScans = await _robotService.GetAllScansAsync();
            if (robotScans is null || robotScans.Scans.Count == 0)
                return NotFound("No scans available from robot.");

            var results = new List<DiseaseScanResponse>();
            using var httpClient = new HttpClient();

            foreach (var robotScan in robotScans.Scans)
            {
                var result = await ProcessSingleScan(robotScan, httpClient, robotBaseUrl, currentUserId!);
                if (result is not null)
                    results.Add(result);
            }

            return Ok(new { Total = results.Count, Results = results });
        }

        // ✅ إحصائيات الـ Robot
        [HttpGet("Stats")]
        public async Task<IActionResult> Stats()
        {
            var stats = await _robotService.GetStatsAsync();
            if (stats is null)
                return StatusCode(500, "Failed to get robot stats.");
            return Ok(stats);
        }

        // ✅ Status الـ Robot
        [HttpGet("Status")]
        public async Task<IActionResult> Status()
        {
            var status = await _robotService.GetRobotStatusAsync();
            if (status is null)
                return StatusCode(500, "Failed to get robot status.");
            return Ok(status);
        }

        // ✅ Start الـ Robot (Admin فقط)
        [HttpPost("Start")]
        [Authorize(Roles = $"{SD.SuperAdmin},{SD.Admin}")]
        public async Task<IActionResult> Start()
        {
            await _robotService.StartRobotAsync();
            return Ok(new { message = "Robot started successfully." });
        }

        // ✅ Stop الـ Robot (Admin فقط)
        [HttpPost("Stop")]
        [Authorize(Roles = $"{SD.SuperAdmin},{SD.Admin}")]
        public async Task<IActionResult> Stop()
        {
            await _robotService.StopRobotAsync();
            return Ok(new { message = "Robot stopped successfully." });
        }
    }
}