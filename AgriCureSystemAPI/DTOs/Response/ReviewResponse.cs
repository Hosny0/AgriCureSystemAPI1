namespace AgriCureSystemAPI.DTOs.Response
{
    public class ReviewResponse
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public int RatingValue { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
