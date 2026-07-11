using AgriCureSystemAPI.DTOs.Response;
using System.Text.Json;

namespace AgriCureSystemAPI.Services
{
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

            Console.WriteLine($"plant_name: {plantName}");
            Console.WriteLine($"File: {image.FileName} - Size: {image.Length}");

            var aiUrl = _configuration["AiApi:BaseUrl"];
            var response = await client.PostAsync($"{aiUrl}/predict", formData);

            var responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"AI Status: {response.StatusCode}");
            Console.WriteLine($"AI Response: {responseBody}");

            if (!response.IsSuccessStatusCode) return null;

            return JsonSerializer.Deserialize<AiPredictionResponse>(responseBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
    }
}