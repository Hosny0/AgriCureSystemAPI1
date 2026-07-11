using AgriCureSystemAPI.DTOs.Response;
using System.Text.Json;

namespace AgriCureSystemAPI.Services
{
    public class RobotService : IRobotService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private JsonSerializerOptions JsonOptions => new() { PropertyNameCaseInsensitive = true };
        private string BaseUrl => _configuration["RobotApi:BaseUrl"]!;

        public RobotService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<RobotLatestResponse?> GetLatestScansAsync()
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{BaseUrl}/latest");
            if (!response.IsSuccessStatusCode) return null;
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<RobotLatestResponse>(json, JsonOptions);
        }

        public async Task<RobotStatsResponse?> GetStatsAsync()
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{BaseUrl}/api/stats");
            if (!response.IsSuccessStatusCode) return null;
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<RobotStatsResponse>(json, JsonOptions);
        }

        public async Task<object?> GetRobotStatusAsync()
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{BaseUrl}/api/robot/status");
            if (!response.IsSuccessStatusCode) return null;
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<object>(json, JsonOptions);
        }

        public async Task StartRobotAsync()
        {
            var client = _httpClientFactory.CreateClient();
            await client.PostAsync($"{BaseUrl}/api/robot/start", null);
        }

        public async Task StopRobotAsync()
        {
            var client = _httpClientFactory.CreateClient();
            await client.PostAsync($"{BaseUrl}/api/robot/stop", null);
        }
    }
}