using System.Text.Json.Serialization;

namespace AgriCureSystemAPI.DTOs.Response
{
    public class PlantClassifierResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("plant_name_en")]
        public string PlantNameEn { get; set; } = string.Empty;

        [JsonPropertyName("plant_name_ar")]
        public string PlantNameAr { get; set; } = string.Empty;

        [JsonPropertyName("confidence")]
        public string Confidence { get; set; } = string.Empty;

        [JsonPropertyName("is_valid_plant")]
        public bool IsValidPlant { get; set; }
    }
}