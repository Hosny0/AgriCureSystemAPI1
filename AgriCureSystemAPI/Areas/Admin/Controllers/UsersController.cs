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
            // 1. جلب كل المستخدمين
            var users = await _userManager.Users.ToListAsync();
            var responseList = new List<UserResponse>();

            // تعريف العدادات
            int adminCount = 0;
            int blockedCount = 0;
            int activeCount = 0;

            foreach (var user in users)
            {
                var userDto = user.Adapt<UserResponse>();

                var roles = await _userManager.GetRolesAsync(user);
                userDto.Role = roles.FirstOrDefault() ?? "No Role";

                if (roles.Contains(SD.Admin) || roles.Contains(SD.SuperAdmin))
                {
                    adminCount++;
                }

                if (user.LockoutEnd != null && user.LockoutEnd > DateTime.UtcNow)
                {
                    blockedCount++;
                }
                else
                {
                    activeCount++;
                }

                responseList.Add(userDto);
            }
            var statistics = new
            {
                TotalCount = users.Count,
                ActiveCount = activeCount,
                BlockedCount = blockedCount,
                AdminCount = adminCount,
                Data = responseList 
            };

            return Ok(statistics);
        }

        [HttpPut("LockUnLock/{id}")]
        public async Task<IActionResult> LockUnLock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user is null)
                return NotFound();

            if (await _userManager.IsInRoleAsync(user, SD.SuperAdmin))
                return BadRequest(new ErrorModelResponse()
                {
                    Code = "Error",
                    Description = "You cannot block a SuperAdmin account"
                });

            string message;

            if (user.LockoutEnd != null && user.LockoutEnd > DateTime.UtcNow)
            {
                user.LockoutEnd = null;
                user.LockoutEnabled = true; 
                message = $"{user.FirstName} has been unlocked successfully";
            }
            else
            {
                user.LockoutEnabled = true; 
                user.LockoutEnd = DateTimeOffset.UtcNow.AddDays(30); 
                message = $"{user.FirstName} has been locked successfully";
            }

            await _userManager.UpdateAsync(user);

            return Ok(new { Message = message });
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