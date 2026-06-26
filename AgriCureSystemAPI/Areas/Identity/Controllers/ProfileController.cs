using AgriCureSystemAPI.DTOs.Request;
using AgriCureSystemAPI.DTOs.Response;
using AgriCureSystemAPI.Models;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AgriCureSystemAPI.Areas.Identity.Controllers
{
    [Route("api/[area]/[controller]")]
    [Area("Identity")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetInfo()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null)
                return NotFound();

            //var userVM = new ApplicationUserVM()
            //{
            //    FullName = user.FirstName + " " + user.LastName,
            //    // FullName = $"{user.FirstName} {user.LastName}",
            //    Address = user.Address,
            //    Email = user.Email,
            //    PhoneNumber = user.PhoneNumber
            //};

            var userVM = user.Adapt<ApplicationUserResponse>();

            return Ok(userVM);
        }
        [HttpPut("UpdateProfile")]
        public async Task<IActionResult> UpdateProfile(ApplicationUserRequest applicationUserRequest)
        {
            var userId = _userManager.GetUserId(User);
            if (userId is null) return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null) return NotFound();

            // ✅ لوج كل خطوة عشان نشوف فين المشكلة
            Console.WriteLine($"=== BEFORE UPDATE ===");
            Console.WriteLine($"UserName in DB: {user.UserName}");
            Console.WriteLine($"UserName requested: {applicationUserRequest.UserName}");

            if (!string.IsNullOrEmpty(applicationUserRequest.UserName)
                && applicationUserRequest.UserName != user.UserName)
            {
                var result = await _userManager.SetUserNameAsync(user, applicationUserRequest.UserName);

                Console.WriteLine($"SetUserNameAsync result: {result.Succeeded}");
                if (!result.Succeeded)
                {
                    Console.WriteLine($"Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    return BadRequest(result.Errors);
                }
            }

            user = await _userManager.FindByIdAsync(userId);

            Console.WriteLine($"=== AFTER SetUserName ===");
            Console.WriteLine($"UserName in DB now: {user.UserName}");
            Console.WriteLine($"NormalizedUserName: {user.NormalizedUserName}");

            user.FirstName = applicationUserRequest.FirstName;
            user.LastName = applicationUserRequest.LastName;
            user.PhoneNumber = applicationUserRequest.PhoneNumber;
            user.Address = applicationUserRequest.Address;
            user.EmailConfirmed = true;

            var updateResult = await _userManager.UpdateAsync(user);

            Console.WriteLine($"UpdateAsync result: {updateResult.Succeeded}");

            return Ok(new
            {
                msg = "Profile updated successfully",
                newUserName = user.UserName,
                newNormalizedUserName = user.NormalizedUserName,
                userId = user.Id
            });
        }

    }
    
}
        //  }

    

