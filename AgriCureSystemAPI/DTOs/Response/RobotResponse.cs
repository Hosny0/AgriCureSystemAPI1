using System.Text.Json.Serialization;

namespace AgriCureSystemAPI.DTOs.Response
{
    public class RobotLatestResponse
    {
        [JsonPropertyName("left")]
        public RobotScanItem? Left { get; set; }

        [JsonPropertyName("right")]
        public RobotScanItem? Right { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("updated_at")]
        public string UpdatedAt { get; set; } = string.Empty;
    }

    public class RobotScanItem
    {
        [JsonPropertyName("direction")]
        public string Direction { get; set; } = string.Empty;

        [JsonPropertyName("image_base64")]
        public string ImageBase64 { get; set; } = string.Empty;

        [JsonPropertyName("size_bytes")]
        public int SizeBytes { get; set; }
    }

    public class RobotStatsResponse
    {
        [JsonPropertyName("total_scans")]
        public int TotalScans { get; set; }

        [JsonPropertyName("diseased")]
        public int Diseased { get; set; }

        [JsonPropertyName("healthy")]
        public int Healthy { get; set; }

        [JsonPropertyName("health_rate")]
        public string HealthRate { get; set; } = string.Empty;

        [JsonPropertyName("top_diseases")]
        public List<List<object>> TopDiseases { get; set; } = new();

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
    }
}