using AgriCureSystemAPI.Models;

namespace AgriCureSystemAPI.Repositories.IRepositories
{
    public interface IDiseaseScanRepository : IRepository<DiseaseScan>
    {
        Task<IEnumerable<DiseaseScan>> GetUserScansAsync(string userId);
    }
}
