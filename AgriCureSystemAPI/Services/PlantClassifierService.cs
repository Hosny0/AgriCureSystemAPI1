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
            formData.Add(new ByteArrayContent(imageBytes), "file", fileName);

            var baseUrl = _configuration["PlantClassifierApi:BaseUrl"];
            var response = await client.PostAsync($"{baseUrl}/predict", formData);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PlantClassifierResponse>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
    }
}