using System.Text.Json.Serialization;

namespace AgriCureSystemAPI.DTOs.Response
{
    public class RobotScanListResponse
    {
        [JsonPropertyName("scans")]
        public List<RobotScanItem> Scans { get; set; } = new();

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("total")]
        public int Total { get; set; }
    }

    public class RobotScanItem
    {
        [JsonPropertyName("scan_id")]
        public string ScanId { get; set; } = string.Empty;

        [JsonPropertyName("image_url")]
        public string ImageUrl { get; set; } = string.Empty;

        [JsonPropertyName("disease")]
        public string Disease { get; set; } = string.Empty;

        [JsonPropertyName("disease_ar")]
        public string DiseaseAr { get; set; } = string.Empty;

        [JsonPropertyName("confidence")]
        public int Confidence { get; set; }

        [JsonPropertyName("direction")]
        public string Direction { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("recommendation")]
        public string Recommendation { get; set; } = string.Empty;

        [JsonPropertyName("captured_at")]
        public string CapturedAt { get; set; } = string.Empty;
    }

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