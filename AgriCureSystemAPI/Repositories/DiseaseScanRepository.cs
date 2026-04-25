using AgriCureSystemAPI.Data;
using AgriCureSystemAPI.Models;
using AgriCureSystem.Repositories.IRepositories;
using AgriCureSystemAPI.Repositories;

namespace AgriCureSystem.Repositories
{
    public class DiseaseScanRepository : Repository<DiseaseScan>, IDiseaseScanRepository
    {
        private readonly ApplicationDbContext _context;

        public DiseaseScanRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
