using AgriCureSystemAPI.DTOs.Response;
using AgriCureSystemAPI.Services;
using System.Text.Json;

public class AiService : IAiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public AiService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<AiPredictionResponse?> PredictDiseaseAsync(IFormFile image, string plantName)
    {
        var client = _httpClientFactory.CreateClient();
        using var formData = new MultipartFormDataContent();
        using var imageStream = image.OpenReadStream();

        formData.Add(new StringContent(plantName), "plant_name");
        formData.Add(new StreamContent(imageStream), "file", image.FileName);

        var aiUrl = _configuration["AiApi:BaseUrl"];
        var response = await client.PostAsync($"{aiUrl}/predict", formData);

        if (!response.IsSuccessStatusCode)
            return null;

        // ✅ parse الـ JSON بدل ما نرجعه string
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<AiPredictionResponse>(json);
    }
}