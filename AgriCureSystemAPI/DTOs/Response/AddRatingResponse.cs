namespace AgriCureSystemAPI.DTOs.Response
{
    public class AddRatingResponse
    {
        public string Message { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public double NewRate { get; set; }
        public int ReviewsCount { get; set; }
    }
}
