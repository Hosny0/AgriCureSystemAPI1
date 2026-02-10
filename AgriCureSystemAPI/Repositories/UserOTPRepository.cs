using AgriCureSystemAPI.Data;
using AgriCureSystemAPI.Models;
using AgriCureSystemAPI.Repositories.IRepositories;

namespace AgriCureSystemAPI.Repositories
{
    public class UserOTPRepository : Repository<UserOTP>, IUserOTPRepository
    {
        private readonly ApplicationDbContext _context;

        public UserOTPRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
