using AgriCureSystemAPI.DTOs.Response;

namespace AgriCureSystemAPI.Services
{
    public interface IAiService
    {
        Task<string?> PredictDiseaseAsync(IFormFile image, string plantName);
    }
}
