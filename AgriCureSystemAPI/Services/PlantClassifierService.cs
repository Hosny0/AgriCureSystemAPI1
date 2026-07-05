using AgriCureSystemAPI.DTOs.Response;
using System.Text.Json;

namespace AgriCureSystemAPI.Services
{
    public class PlantClassifierService : IPlantClassifierService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public PlantClassifierService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<PlantClassifierResponse?> ClassifyPlantAsync(byte[] imageBytes, string fileName)
        {
            var client = _httpClientFactory.CreateClient();
            using var formData = new MultipartFormDataContent();

            // ✅ زود الـ ContentType صريح
            var imageContent = new ByteArrayContent(imageBytes);
            imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            formData.Add(imageContent, "file", fileName);

            var baseUrl = _configuration["PlantClassifierApi:BaseUrl"];

            Console.WriteLine($"URL: {baseUrl}/predict");
            Console.WriteLine($"FileName: {fileName}");
            Console.WriteLine($"Size: {imageBytes.Length} bytes");

            var response = await client.PostAsync($"{baseUrl}/predict", formData);

            var responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"StatusCode: {response.StatusCode}");
            Console.WriteLine($"ResponseBody: {responseBody}");

            if (!response.IsSuccessStatusCode) return null;

            return JsonSerializer.Deserialize<PlantClassifierResponse>(responseBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
    }
}