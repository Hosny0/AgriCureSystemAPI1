using System.Text.Json.Serialization;

namespace AgriCureSystemAPI.DTOs.Response
{
    public class AiPredictionResponse
    {
        [JsonPropertyName("plant")]
        public string Plant { get; set; } = string.Empty;

        [JsonPropertyName("prediction")]
        public string Prediction { get; set; } = string.Empty;

        [JsonPropertyName("confidence")]
        public string Confidence { get; set; } = string.Empty;

        [JsonPropertyName("details")]
        public AiPredictionDetails Details { get; set; } = new();
    }

    public class AiPredictionDetails
    {
        [JsonPropertyName("Description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("Symptoms")]
        public string Symptoms { get; set; } = string.Empty;

        [JsonPropertyName("Treatment")]
        public string Treatment { get; set; } = string.Empty;
    }
}