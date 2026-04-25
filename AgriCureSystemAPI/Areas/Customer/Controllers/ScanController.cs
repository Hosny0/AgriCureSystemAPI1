using AgriCureSystem.Repositories.IRepositories;
using AgriCureSystem.Services;
using AgriCureSystemAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AgriCureSystemAPI.Areas.Customer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class ScanController : ControllerBase
    {
        private readonly PlantDiseaseApiService _aiService;
        private readonly IDiseaseScanRepository _repo;

        public ScanController(PlantDiseaseApiService aiService, IDiseaseScanRepository repo)
        {
            _aiService = aiService;
            _repo = repo;
        }

        
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var myScans = await _repo.GetAsync(filter: s => s.UserId == currentUserId);

            return Ok(myScans.OrderByDescending(s => s.ScanDate));
        }

        [HttpPost("CheckPlant")]
        public async Task<IActionResult> CheckPlant([FromForm] string plantName, IFormFile image)
        {
            if (image == null || image.Length == 0)
                return BadRequest("Please upload an image.");

            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var aiResult = await _aiService.DetectDiseaseAsync(plantName, image);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\ScanImage", fileName);

                using (var stream = System.IO.File.Create(filePath))
                {
                    await image.CopyToAsync(stream);
                }

                var newScan = new DiseaseScan
                {
                    PlantName = aiResult.Plant,
                    DiseaseName = aiResult.Prediction,
                    ConfidenceRate = aiResult.Confidence,
                    ScanDate = DateTime.UtcNow,
                    ImageUrl = fileName,
                    UserId = currentUserId 
                };

                await _repo.CreateAsync(newScan);
                await _repo.CommitAsync();

                return Ok(aiResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
