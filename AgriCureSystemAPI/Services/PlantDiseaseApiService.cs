using AgriCureSystemAPI.Models;
using AgriCureSystemAPI.DTOs.Response;
using System.Net.Http.Headers;
using System.Text.Json;

namespace AgriCureSystem.Services
{
    public class PlantDiseaseApiService
    {
        private readonly HttpClient _httpClient;

        public PlantDiseaseApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<AiPredictionResponse> DetectDiseaseAsync(string plantName, IFormFile imageFile)
        {
            using var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(plantName), "plant_name");

            using var stream = imageFile.OpenReadStream();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(imageFile.ContentType);
            formData.Add(fileContent, "file", imageFile.FileName);

            var response = await _httpClient.PostAsync("predict", formData);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<AiPredictionResponse>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
    }
}