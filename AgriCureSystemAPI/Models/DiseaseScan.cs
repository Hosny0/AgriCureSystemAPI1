namespace AgriCureSystemAPI.Models
{
    public class DiseaseScan
    {
       
            public int Id { get; set; }
            public string PlantName { get; set; } = string.Empty;
            public string DiseaseName { get; set; } = string.Empty;
            public string ConfidenceRate { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;  // ✅
            public string Symptoms { get; set; } = string.Empty;     // ✅
            public string Treatment { get; set; } = string.Empty;    // ✅
            public DateTime ScanDate { get; set; } = DateTime.UtcNow;
            public string ImageUrl { get; set; } = string.Empty;
            public string UserId { get; set; } = string.Empty;
            public ApplicationUser User { get; set; } = null!;
        
    }
}
