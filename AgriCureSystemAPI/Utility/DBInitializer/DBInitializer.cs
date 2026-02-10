using AgriCureSystemAPI.Data;
using AgriCureSystemAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;

namespace AgriCureSystemAPI.Utility.DBInitializer
{
    public class DBInitializer : IDBInitializer
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DBInitializer> _logger;

        public DBInitializer(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context, ILogger<DBInitializer> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _logger = logger;
        }

        public void Initialize()
        {
            try
            {
                if(_context.Database.GetPendingMigrations().Any())
                {
                    _context.Database.Migrate();
                }

                if (_roleManager.Roles.IsNullOrEmpty())
                {
                    _roleManager.CreateAsync(new(SD.SuperAdmin)).GetAwaiter().GetResult();
                    _roleManager.CreateAsync(new(SD.Admin)).GetAwaiter().GetResult();
                    _roleManager.CreateAsync(new(SD.Employee)).GetAwaiter().GetResult();
                    _roleManager.CreateAsync(new(SD.Customer)).GetAwaiter().GetResult();

                    _userManager.CreateAsync(new()
                    {
                        UserName = "SuperAdmin",
                        Email = "SuperAdmin@AgriCureSystemAPI.com",
                        FirstName = "Super",
                        LastName = "Admin",
                        EmailConfirmed = true
                    }, "Admin123$").GetAwaiter().GetResult();

                    var user = _userManager.FindByNameAsync("SuperAdmin").GetAwaiter().GetResult();

                    _userManager.AddToRoleAsync(user, SD.SuperAdmin).GetAwaiter().GetResult();
                }
                //
            }
            catch(Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
