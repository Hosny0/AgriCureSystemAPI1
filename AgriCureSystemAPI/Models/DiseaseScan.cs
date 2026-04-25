namespace AgriCureSystemAPI.Models
{
    public class DiseaseScan
    {
        public int Id { get; set; }
        public string PlantName { get; set; }
        public string DiseaseName { get; set; }
        public string ConfidenceRate { get; set; }
        public DateTime ScanDate { get; set; }
        public string ImageUrl { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
