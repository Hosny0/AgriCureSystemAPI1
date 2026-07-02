namespace AgriCureSystemAPI.DTOs.Response
{
    public class DiseaseScanResponse
    {
        public int Id { get; set; }
        public string PlantName { get; set; } = string.Empty;
        public string DiseaseName { get; set; } = string.Empty;
        public string ConfidenceRate { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Symptoms { get; set; } = string.Empty;
        public string Treatment { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public DateTime ScanDate { get; set; }
    }
}