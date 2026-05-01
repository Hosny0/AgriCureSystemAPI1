using AgriCureSystemAPI.Data;
using AgriCureSystemAPI.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AgriCureSystemAPI.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly DbSet<T> _db;
        private readonly ApplicationDbContext _context;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _db = _context.Set<T>();
        }

        public async Task CreateAsync(T entity) => await _db.AddAsync(entity);
        public void Edit(T entity) => _db.Update(entity);
        public void Delete(T entity) => _db.Remove(entity);

        public async Task<IEnumerable<T>> GetAsync(
            Expression<Func<T, bool>>? filter = null,
            Expression<Func<T, object>>[]? includes = null,
            string? includeProperties = null,
            bool tracked = true)
        {
            IQueryable<T> query = _db;

            if (filter is not null) query = query.Where(filter);

            // تطبيق الـ Includes (Expressions)
            if (includes is not null)
            {
                foreach (var item in includes) query = query.Include(item);
            }

            // تطبيق الـ Includes (Strings) عشان الـ Nested Data
            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp.Trim());
                }
            }

            if (!tracked) query = query.AsNoTracking();

            return await query.ToListAsync();
        }

        public async Task<T?> GetOneAsync(
            Expression<Func<T, bool>>? filter = null,
            Expression<Func<T, object>>[]? includes = null,
            string? includeProperties = null,
            bool tracked = true)
        {
            return (await GetAsync(filter, includes, includeProperties, tracked)).FirstOrDefault();
        }

        public async Task<bool> CommitAsync()
        {
            try { await _context.SaveChangesAsync(); return true; }
            catch { return false; }
        }
    }
}