using AgriCureSystemAPI.DTOs.Response;

namespace AgriCureSystemAPI.Services
{
    public interface IAiService
    {
        Task<AiPredictionResponse?> PredictDiseaseAsync(IFormFile image, string plantName);
    }
}
