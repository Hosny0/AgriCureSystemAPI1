using AgriCureSystemAPI.Models;
using AgriCureSystemAPI.Repositories.IRepositories;

namespace AgriCureSystemAPI.Repositories.IRepositories
{
    public interface IReviewRepository : IRepository<Review>
    {
        Task<Review?> GetUserReviewAsync(int productId, string userId);
    }
}
