using AgriCureSystemAPI.DTOs.Request;
using AgriCureSystemAPI.DTOs.Response;
using AgriCureSystemAPI.Models;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AgriCureSystemAPI.Areas.Identity
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
            var user = await _userManager.GetUserAsync(User);

            if (user is null)
                return NotFound();

            user.FirstName = applicationUserRequest.Name;
            user.UserName = applicationUserRequest.UserName;
            user.Email = applicationUserRequest.Email;
            user.PhoneNumber = applicationUserRequest.PhoneNumber;
            user.Address = applicationUserRequest.Address;

            await _userManager.UpdateAsync(user);

            return Ok(new
            {
                msg ="Update profile "
            });
        }

        [HttpPut("ChangePassword")]
        public async Task<IActionResult> ChangePassword(ApplicationUserRequest applicationUserRequest)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null)
                return NotFound();

            var result = await _userManager.ChangePasswordAsync(user, applicationUserRequest.OldPassword, applicationUserRequest.NewPassword);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new
            {
                msg = "Update profile "
            });
        }
    }
}

