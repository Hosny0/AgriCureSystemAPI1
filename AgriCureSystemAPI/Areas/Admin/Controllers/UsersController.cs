using AgriCureSystemAPI.DTOs.Response;
using AgriCureSystemAPI.Models;
using AgriCureSystemAPI.Utility;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AgriCureSystemAPI.Areas.Admin.Controllers
{
    [Route("api[area]/[controller]")]
    [ApiController]
    [Area("Admin")]
    [Authorize(Roles = $"{SD.SuperAdmin},{SD.Admin}")]

    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet("Index")]
        public IActionResult Index()
        {
            // var users = _userManager.Users.AsNoTracking().AsQueryable();

            return Ok(_userManager.Users.Adapt<IEnumerable<UserResponse>>());
        }
        [HttpPut("{id}")]

        public async Task<IActionResult> LockUnLock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user is null)
                return NotFound();

            if (await _userManager.IsInRoleAsync(user, SD.SuperAdmin))
            {
                return BadRequest(new ErrorModelResponse()
                {
                    Code = "Error",
                    Description = "you can not block superAdmin account"
                });
            }

            user.LockoutEnabled = !user.LockoutEnabled;

            if (user.LockoutEnabled)
                user.LockoutEnd = DateTime.UtcNow.AddDays(30);
            else
                user.LockoutEnd = null;

            await _userManager.UpdateAsync(user);

            return NoContent();
        }
    }
}
