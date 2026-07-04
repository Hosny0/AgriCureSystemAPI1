using AgriCureSystemAPI.DTOs.Response;

namespace AgriCureSystemAPI.Services
{
    public interface IPlantClassifierService
    {
        Task<PlantClassifierResponse?> ClassifyPlantAsync(byte[] imageBytes, string fileName);
    }
}