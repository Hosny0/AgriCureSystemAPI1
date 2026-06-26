using AgriCureSystemAPI.DTOs.Request;
using AgriCureSystemAPI.Models;
using AgriCureSystemAPI.Repositories.IRepositories;
using AgriCureSystemAPI.Services;
using AgriCureSystemAPI.Utility;
using Azure.Core;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities; // إضافة ضرورية للـ WebEncoders
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AgriCureSystemAPI.Areas.Identity.Controllers
{
    [Route("api/[area]/[controller]")]
    [Area("Identity")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IUserOTPRepository _userOTPRepository;
        private readonly ITokenServices _tokenServices;

        public AccountController(UserManager<ApplicationUser> userManager, IEmailSender emailSender, SignInManager<ApplicationUser> signInManager, IUserOTPRepository userOTPRepository, ITokenServices tokenServices)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _signInManager = signInManager;
            _userOTPRepository = userOTPRepository;
            _tokenServices = tokenServices;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterRequest registerRequest)
        {
            ApplicationUser applicationUser = registerRequest.Adapt<ApplicationUser>();

            var result = await _userManager.CreateAsync(applicationUser, registerRequest.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(applicationUser, SD.Customer);

                var token = await _userManager.GenerateEmailConfirmationTokenAsync(applicationUser);

                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

                var link = Url.Action(nameof(ConfirmEmail), "Account", new { area = "Identity", token = encodedToken, userId = applicationUser.Id }, Request.Scheme);
                await _emailSender.SendEmailAsync(registerRequest.Email, "Confirm Your Account", $"<h1>Confirm Your Account By Clicking <a href='{link}'>Here</a></h1>");

                return Ok("Add Account Successfully, Confirm Your Account!");
            }

            foreach (var item in result.Errors)
            {
                ModelState.AddModelError(string.Empty, item.Description);
            }

            return BadRequest(ModelState);
        }

        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user is not null)
            {
                // فك التشفير عشان نرجع التوكن لأصله قبل ما نديه للـ Identity
                var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));

                // استخدام التوكن بعد فك التشفير
                var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

                if (result.Succeeded)
                    return Ok("Email Confirmed Successfully!");

                return BadRequest(result.Errors);
            }

            return NotFound("User not found.");
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            // 1. البحث عن المستخدم
            var user = await _userManager.FindByNameAsync(loginRequest.EmailORUserName)
                    ?? await _userManager.FindByEmailAsync(loginRequest.EmailORUserName);

            if (user is null)
                return BadRequest("Invalid User Name / Email OR Password");

            // 2. التحقق من الـ Lockout يدوياً
            if (await _userManager.IsLockedOutAsync(user))
            {
                var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
                var remainingDays = (lockoutEnd!.Value - DateTimeOffset.UtcNow).Days;
                if (remainingDays > 0)
                    return BadRequest($"Your account is suspended. Try again after {remainingDays} days.");
                else
                    return BadRequest("Too many attempts, try again after a few minutes.");
            }

            // 3. التحقق من الإيميل
            if (!user.EmailConfirmed)
                return BadRequest("Please Confirm Your Email First!!");

            // ✅ 4. CheckPasswordAsync بدل PasswordSignInAsync
            // عشان مش بتتأثرش بالـ Security Stamp
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, loginRequest.Password);

            if (!isPasswordValid)
            {
                // زيادة عداد المحاولات الفاشلة يدوياً
                await _userManager.AccessFailedAsync(user);
                return BadRequest("Invalid User Name / Email OR Password");
            }

            // 5. reset عداد المحاولات الفاشلة
            await _userManager.ResetAccessFailedCountAsync(user);

            // 6. إنشاء التوكنز
            var userRoles = await _userManager.GetRolesAsync(user);
            var claims = new[]
            {
        new Claim(ClaimTypes.Name, user.UserName!),
        new Claim(ClaimTypes.Email, user.Email!),
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Role, String.Join(", ", userRoles)),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
    };

            var accessToken = _tokenServices.GenerateAccessToken(claims);
            var refreshToken = _tokenServices.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            return Ok(new
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ValidTo = "30 min",
                RefreshTokenExpiration = "7 days"
            });
        }



        [HttpPost("ResendEmailConfirmation")]
        public async Task<IActionResult> ResendEmailConfirmation(ResendEmailConfirmationRequest resendEmailConfirmationRequest)
        {
            var user = await _userManager.FindByEmailAsync(resendEmailConfirmationRequest.EmailORUserName) ?? await _userManager.FindByNameAsync(resendEmailConfirmationRequest.EmailORUserName);

            if (user is not null)
            {
                // إنشاء التوكن وتشفيره هنا أيضاً
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

                var link = Url.Action(nameof(ConfirmEmail), "Account", new { area = "Identity", token = encodedToken, userId = user.Id }, Request.Scheme);
                await _emailSender.SendEmailAsync(user.Email!, "Confirm Your Account", $"<h1>Confirm Your Account By Clicking <a href='{link}'>Here</a></h1>");

                return Ok("Confirm Your Account Again!");
            }

            return NotFound();
        }

        [HttpPost("ForgetPassword")]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordRequest forgetPasswordRequest)
        {
            var user = await _userManager.FindByEmailAsync(forgetPasswordRequest.EmailORUserName) ?? await _userManager.FindByNameAsync(forgetPasswordRequest.EmailORUserName);

            if (user is not null)
            {
                var userOTP = await _userOTPRepository.GetAsync(e => e.ApplicationUserId == user.Id);
                var totalOTPs = userOTP.Count(e => e.Date.Day == DateTime.UtcNow.Day && e.Date.Month == DateTime.UtcNow.Month && e.Date.Year == DateTime.UtcNow.Year);

                if (totalOTPs < 3)
                {
                    var OTPNumber = new Random().Next(1000, 9999);
                    await _emailSender.SendEmailAsync(user.Email!, "Reset Password", $"<h1>Reset Password Using OTP Number {OTPNumber}</h1>");

                    await _userOTPRepository.CreateAsync(new()
                    {
                        Code = OTPNumber.ToString(),
                        Date = DateTime.UtcNow,
                        ExpirationDate = DateTime.UtcNow.AddHours(1),
                        ApplicationUserId = user.Id
                    });
                    await _userOTPRepository.CommitAsync();

                    // تم تعديل الكلمة من Accounts لـ Account
                    return CreatedAtAction(nameof(ResetPassword), "Account", new { area = "Identity", userId = user.Id! }, string.Empty);
                }
                return BadRequest("Too Many Request, Please try again Later");
            }

            return NotFound("User not found");
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest resetPasswordRequest)
        {
            // 1. الوصول للمستخدم عن طريق الـ UserName
            var user = await _userManager.FindByNameAsync(resetPasswordRequest.UserName);

            if (user is null)
                return BadRequest("User not found");

            // 2. البحث عن الـ OTP باستخدام الـ Id الداخلي للمستخدم
            var userOTP = (await _userOTPRepository.GetAsync(e => e.ApplicationUserId == user.Id))
                          .OrderBy(e => e.Id)
                          .LastOrDefault();

            if (userOTP is not null)
            {
                if (DateTime.UtcNow < userOTP.ExpirationDate && !userOTP.Status && userOTP.Code == resetPasswordRequest.Code)
                {
                    // نمرر الـ UserName للـ Action القادم
                    return CreatedAtAction(nameof(ChangePassword), "Account", new { area = "Identity", userName = user.UserName }, string.Empty);
                }
            }

            return BadRequest("Invalid Code");
        }

        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest changePasswordRequest)
        {
            // البحث بالـ UserName بدل الـ Id
            var user = await _userManager.FindByNameAsync(changePasswordRequest.UserName);

            if (user is not null)
            {
                // توليد الـ Token وإعادة تعيين الكلمة
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, changePasswordRequest.Password);

                if (result.Succeeded)
                {
                    return Ok("Reset Password Successfully");
                }

                return BadRequest(result.Errors);
            }

            return NotFound("User not found");
        }
        [HttpPost]
        [Route("refresh")]
        public async Task<IActionResult> Refresh(TokenApiRequest tokenApiRequest)
        {
            if (tokenApiRequest is null || tokenApiRequest.AccessToken is null || tokenApiRequest.RefreshToken is null)
                return BadRequest("Invalid client request");

            string accessToken = tokenApiRequest.AccessToken;
            string refreshToken = tokenApiRequest.RefreshToken;

            var principal = _tokenServices.GetPrincipalFromExpiredToken(accessToken);
            var userName = principal.Identity.Name;

            var user = _userManager.Users.FirstOrDefault(u => u.UserName == userName);
            if (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
                return BadRequest("Invalid client request");

            var newAccessToken = _tokenServices.GenerateAccessToken(principal.Claims);
            var newRefreshToken = _tokenServices.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            await _userManager.UpdateAsync(user);

            return Ok(new
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ValidTo = "30 min",
            });
        }

        [HttpPost, Authorize]
        [Route("revoke")]
        public async Task<IActionResult> Revoke()
        {
            var username = User.Identity.Name;
            var user = _userManager.Users.FirstOrDefault(u => u.UserName == username);
            if (user == null) return BadRequest();
            user.RefreshToken = null;
            await _userManager.UpdateAsync(user);
            return NoContent();
        }
    }
}