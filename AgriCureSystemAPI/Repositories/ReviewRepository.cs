using AgriCureSystemAPI.Data;
using AgriCureSystemAPI.Models;
using AgriCureSystemAPI.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace AgriCureSystemAPI.Repositories
{
    public class ReviewRepository : Repository<Review>, IReviewRepository
    {
        private readonly ApplicationDbContext _context;

        public ReviewRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<Review?> GetUserReviewAsync(int productId, string userId)
        {
            return await _context.Reviews
                .FirstOrDefaultAsync(r => r.ProductId == productId && r.UserId == userId);
        }
    }
}
