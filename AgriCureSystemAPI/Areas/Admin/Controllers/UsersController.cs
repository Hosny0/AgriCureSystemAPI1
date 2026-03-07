using AgriCureSystemAPI.DTOs.Request;
using AgriCureSystemAPI.DTOs.Response;
using AgriCureSystemAPI.Models;
using AgriCureSystemAPI.Utility;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgriCureSystemAPI.Areas.Admin.Controllers
{
    [Route("api/[area]/[controller]")]
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
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            return Ok(users.Adapt<IEnumerable<UserResponse>>());
        }

        [HttpPut("LockUnLock/{id}")]
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
                    Description = "You cannot block a SuperAdmin account"
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

        [HttpPut("UpdateRole/{userId}")]
        public async Task<IActionResult> UpdateRole(string userId, [FromBody] UpdateRoleRequest request)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound();

            if (await _userManager.IsInRoleAsync(user, SD.SuperAdmin) && request.RoleName != SD.SuperAdmin)
            {
                return BadRequest(new ErrorModelResponse()
                {
                    Code = "Error",
                    Description = "Super Admin privileges cannot be changed"
                });
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            if (!string.IsNullOrEmpty(request.RoleName))
            {
                await _userManager.AddToRoleAsync(user, request.RoleName);
            }

            return Ok(new
            {
                Message = $"{user.FirstName} permissions updated successfully"
            });
        }
    }
}