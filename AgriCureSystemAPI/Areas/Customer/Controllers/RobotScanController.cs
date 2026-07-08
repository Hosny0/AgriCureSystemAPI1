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
        private readonly IDiseaseScanRepository _diseaseScanRepo;
        private readonly IConfiguration _configuration;

        public RobotScanController(
            IRobotService robotService,
            IDiseaseScanRepository diseaseScanRepo,
            IConfiguration configuration)
        {
            _robotService = robotService;
            _diseaseScanRepo = diseaseScanRepo;
            _configuration = configuration;
        }

        // ✅ Helper — يحفظ scan من Robot مباشرة
        private async Task<DiseaseScanResponse?> ProcessSingleScan(
            RobotScanItem robotScan,
            string robotBaseUrl,
            string currentUserId)
        {
            try
            {
                var fullImageUrl = $"{robotBaseUrl}{robotScan.ImageUrl}";

                // ✅ احفظ في الداتابيز مباشرة من بيانات الـ Robot
                var scan = new DiseaseScan
                {
                    PlantName = robotScan.Disease,
                    DiseaseName = robotScan.Disease,
                    ConfidenceRate = $"{robotScan.Confidence}%",
                    Description = robotScan.Recommendation,
                    Symptoms = string.Empty,
                    Treatment = robotScan.Recommendation,
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
        [HttpGet("ScanLatest")]
        public async Task<IActionResult> ScanLatest()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var robotBaseUrl = _configuration["RobotApi:BaseUrl"]!;

            var latest = await _robotService.GetLatestScansAsync();
            if (latest is null)
                return NotFound("No latest scan available from robot.");

            var results = new List<DiseaseScanResponse>();

            foreach (var robotScan in new[] { latest.Left, latest.Right }.Where(s => s is not null))
            {
                var result = await ProcessSingleScan(robotScan!, robotBaseUrl, currentUserId!);
                if (result is not null)
                    results.Add(result);
            }

            if (results.Count == 0)
                return StatusCode(500, "Failed to process robot scans.");

            return Ok(new { Total = results.Count, Results = results });
        }

        // ✅ كل صور الـ Robot
        [HttpGet("ScanAll")]
        public async Task<IActionResult> ScanAll()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var robotBaseUrl = _configuration["RobotApi:BaseUrl"]!;

            var robotScans = await _robotService.GetAllScansAsync();
            if (robotScans is null || robotScans.Scans.Count == 0)
                return NotFound("No scans available from robot.");

            var results = new List<DiseaseScanResponse>();

            foreach (var robotScan in robotScans.Scans)
            {
                var result = await ProcessSingleScan(robotScan, robotBaseUrl, currentUserId!);
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