using AgriCureSystemAPI.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using AgriCureSystemAPI.Models;

using AgriCureSystemAPI.Repositories.IRepositories;

namespace AgriCureSystemAPI.Repositories
{
    public class DiseaseScanRepository : Repository<DiseaseScan>, IDiseaseScanRepository
    {
        private readonly ApplicationDbContext _context;

        public DiseaseScanRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DiseaseScan>> GetUserScansAsync(string userId)
        {
            return await _context.DiseaseScan
                                 .Where(s => s.UserId == userId)
                                 .ToListAsync();
        }
    }
}
