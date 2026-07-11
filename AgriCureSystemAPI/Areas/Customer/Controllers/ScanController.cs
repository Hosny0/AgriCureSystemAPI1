using AgriCureSystemAPI.Repositories.IRepositories;
using AgriCureSystemAPI.DTOs.Request;
using AgriCureSystemAPI.DTOs.Response;
using AgriCureSystemAPI.Models;
using AgriCureSystemAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Route("api/[area]/[controller]")]
[ApiController]
[Area("Customer")]
[Authorize]
public class DiseaseScanController : ControllerBase
{
    private readonly IDiseaseScanRepository _diseaseScanRepo;
    private readonly IAiService _aiService;
    private readonly IWebHostEnvironment _env;

    public DiseaseScanController(
        IDiseaseScanRepository diseaseScanRepo,
        IAiService aiService,
        IWebHostEnvironment env)
    {
        _diseaseScanRepo = diseaseScanRepo;
        _aiService = aiService;
        _env = env;
    }

    [HttpPost("Scan")]
    public async Task<IActionResult> Scan([FromForm] CreateDiseaseScanRequest request)
    {
        if (request.Image is null || request.Image.Length == 0)
            return BadRequest("Image is required.");

        if (!request.IsValidImage())
            return BadRequest("Image must be jpg/jpeg/png and less than 5MB.");

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(request.Image.FileName);
        var filePath = Path.Combine(_env.WebRootPath, "ScanImage", fileName);
        using (var stream = System.IO.File.Create(filePath))
        {
            await request.Image.CopyToAsync(stream);
        }

        var aiResult = await _aiService.PredictDiseaseAsync(request.Image, request.PlantName);
        if (aiResult is null)
            return StatusCode(500, "AI API error.");

        var scan = new DiseaseScan
        {
            PlantName = aiResult.Plant,
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

    [HttpGet("History")]
    public async Task<IActionResult> History()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var scans = await _diseaseScanRepo.GetUserScansAsync(currentUserId!);

        var response = scans.Select(s => new DiseaseScanResponse
        {
            Id = s.Id,
            PlantName = s.PlantName,
            DiseaseName = s.DiseaseName,
            ConfidenceRate = s.ConfidenceRate,
            Description = s.Description,
            Symptoms = s.Symptoms,
            Treatment = s.Treatment,
            ImageUrl = s.ImageUrl,
            ScanDate = s.ScanDate
        }).ToList();

        return Ok(response);
    }

    [HttpGet("Details/{id}")]
    public async Task<IActionResult> Details(int id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var scan = await _diseaseScanRepo.GetOneAsync(
            s => s.Id == id && s.UserId == currentUserId
        );

        if (scan is null)
            return NotFound();

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

    [HttpDelete("Delete/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var scan = await _diseaseScanRepo.GetOneAsync(
            s => s.Id == id && s.UserId == currentUserId
        );

        if (scan is null)
            return NotFound();

        var filePath = Path.Combine(_env.WebRootPath, "ScanImage", scan.ImageUrl);
        if (System.IO.File.Exists(filePath))
            System.IO.File.Delete(filePath);

        _diseaseScanRepo.Delete(scan);
        var saved = await _diseaseScanRepo.CommitAsync();
        if (!saved)
            return StatusCode(500, "Failed to delete scan.");

        return NoContent();
    }
}