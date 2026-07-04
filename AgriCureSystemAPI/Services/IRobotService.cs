using AgriCureSystemAPI.DTOs.Response;

namespace AgriCureSystemAPI.Services
{
    public interface IRobotService
    {
        Task<RobotScanListResponse?> GetAllScansAsync();
        Task<RobotLatestResponse?> GetLatestScansAsync();
        Task<RobotStatsResponse?> GetStatsAsync();
        Task<object?> GetRobotStatusAsync();
        Task StartRobotAsync();
        Task StopRobotAsync();
    }
}