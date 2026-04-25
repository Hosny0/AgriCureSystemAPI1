namespace AgriCureSystemAPI.DTOs.Response
{
    public class AiPredictionResponse
    {
        public string Plant { get; set; }
        public string Prediction { get; set; }
        public string Confidence { get; set; }
        public AiPredictionDetails Details { get; set; }
    }

    public class AiPredictionDetails
    {
        public string Description { get; set; }
        public string Symptoms { get; set; }
        public string Treatment { get; set; }
    }
}
