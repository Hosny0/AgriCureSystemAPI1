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
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return NotFound();

            // 1. تحديث البيانات العادية
            user.FirstName = applicationUserRequest.FirstName;
            user.LastName = applicationUserRequest.LastName;
            user.PhoneNumber = applicationUserRequest.PhoneNumber;
            user.Address = applicationUserRequest.Address;

            // 2. التحديث الصح للاسم (عشان NormalizedUserName يتحدث واللوجين يشتغل)
            if (applicationUserRequest.UserName != user.UserName)
            {
                var result = await _userManager.SetUserNameAsync(user, applicationUserRequest.UserName);
                if (!result.Succeeded) return BadRequest(result.Errors);
            }

            // 3. التحديث الصح للإيميل
            if (applicationUserRequest.Email != user.Email)
            {
                var result = await _userManager.SetEmailAsync(user, applicationUserRequest.Email);
                if (!result.Succeeded) return BadRequest(result.Errors);
                user.EmailConfirmed = true; // تفعيل فوري عشان الـ Login ميرفضش
            }

            // 4. حفظ التغييرات وتحديث بصمة الأمان
            await _userManager.UpdateAsync(user);
            await _userManager.UpdateSecurityStampAsync(user); // مهمة جداً عشان التوكنز القديمة تبطل والجديدة تشتغل

            return Ok(new { msg = "Profile updated successfully" });

        }

    }
    
      //  [HttpPut("ChangePassword")]
      //  public async Task<IActionResult> ChangePassword(ApplicationUserRequest applicationUserRequest)
      //  {
      //      var user = await _userManager.GetUserAsync(User);
      //
      //      if (user is null)
      //          return NotFound();
      //
      //      var result = await _userManager.ChangePasswordAsync(user, applicationUserRequest.OldPassword, applicationUserRequest.NewPassword);
      //
      //      if (!result.Succeeded)
      //      {
      //          return BadRequest(result.Errors);
      //      }
      //
      //      return Ok(new
      //      {
      //          msg = "Update profile "
      //      });
      //  }
    
}

