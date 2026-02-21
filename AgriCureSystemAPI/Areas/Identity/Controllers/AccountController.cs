using AgriCureSystemAPI.DTOs.Request;
using AgriCureSystemAPI.Models;
using AgriCureSystemAPI.Repositories.IRepositories;
using AgriCureSystemAPI.Utility;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


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
        private ApplicationUser applicationUser;

        public AccountController(UserManager<ApplicationUser> userManager, IEmailSender emailSender, SignInManager<ApplicationUser> signInManager, IUserOTPRepository userOTPRepository)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _signInManager = signInManager;
            _userOTPRepository = userOTPRepository;
        }
        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterRequest registerRequest)
        {


            //  ApplicationUser applicationUser = new()
            //{
            //  UserName = registerVM.UserName,
            //  Email = registerVM.Email,
            //   FirstName = registerVM.FirstName,
            // LastName = registerVM.LastName,
            // Address = registerVM.Address
            //};

            ApplicationUser applicationUser = registerRequest.Adapt<ApplicationUser>();

            var result = await _userManager.CreateAsync(applicationUser, registerRequest.Password);

            if (result.Succeeded)
            {
                // Send Email
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(applicationUser);
                var link = Url.Action(nameof(ConfirmEmail), "Account", new { area = "Identity", token, userId = applicationUser.Id }, Request.Scheme);
                await _emailSender.SendEmailAsync(registerRequest.Email, "Confirm Your Account", $"<h1>Confirm Your Account By Clicking <a href='{link}'>Here</a></h1>");

                await _userManager.AddToRoleAsync(applicationUser, SD.Customer);



            }

            foreach (var item in result.Errors)
            {
                ModelState.AddModelError(string.Empty, item.Description);
            }
            //TempData["error-notification"] = String.Join(", ", result.Errors.Select(e=>e.Description));

            return Ok("Add Account Successfully, Confirm Your Account!");

        }
        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user is not null)
            {
                var result = await _userManager.ConfirmEmailAsync(user, token);

                if (result.Succeeded)
                    return Ok();
                return BadRequest(result.Errors);


            }

            return NotFound();
        }
        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginRequest loginRequest)
        {


            var user = await _userManager.FindByEmailAsync(loginRequest.EmailORUserName) ?? await _userManager.FindByNameAsync(loginRequest.EmailORUserName);

            if (user is not null)
            {
                var result = await _signInManager.PasswordSignInAsync(user.UserName, loginRequest.Password, loginRequest.RememberMe, lockoutOnFailure: true);


                if (result.IsLockedOut)
                {
                    return BadRequest("Too Many Attempts");
                }

                if (result.Succeeded)
                {
                    if (!user.EmailConfirmed)
                    {
                        return BadRequest("Confirm Your Account!");
                    }

                    if (!user.LockoutEnabled)
                    {
                        return BadRequest($"You have a block till {user.LockoutEnd}");
                    }

                    var userRoles = await _userManager.GetRolesAsync(user);

                    var claims = new List<Claim> {
                        new Claim(ClaimTypes.NameIdentifier, user.Id),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Name, user.UserName),
                        new Claim(ClaimTypes.Role, String.Join(",", userRoles)),
                    };

                    var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes("HosnyAshraf**HosnyAshraf**HosnyAshraf**HosnyAshraf**HosnyAshraf")), SecurityAlgorithms.HmacSha256);

                    var token = new JwtSecurityToken(
                        issuer: "https://localhost:7177",
                        audience: "https://localhost:7112,,https://localhost:4200",
                        claims: claims,
                        expires: DateTime.UtcNow.AddDays(1),
                        signingCredentials: signingCredentials
                    ); 

                    return Ok(new
                    {
                        token = new JwtSecurityTokenHandler().WriteToken(token),
                        expires = token.ValidTo
                    });
                }


            }
            return BadRequest("Invalid User Name Or Password");

        }
        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok("Logout Successfully");
        }
        [HttpPost("ResendEmailConfirmation")]
        public async Task<IActionResult> ResendEmailConfirmation(ResendEmailConfirmationRequest resendEmailConfirmationRequest)
        {
            
            var user = await _userManager.FindByEmailAsync(resendEmailConfirmationRequest.EmailORUserName) ?? await _userManager.FindByNameAsync(resendEmailConfirmationRequest.EmailORUserName);

            if (user is not null)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var link = Url.Action(nameof(ConfirmEmail), "Account", new { area = "Identity", token, userId = user.Id }, Request.Scheme);
                await _emailSender.SendEmailAsync(user.Email!, "Confirm Your Account", $"<h1>Confirm Your Account By Clicking <a href='{link}'>Here</a></h1>");
                return Ok("Confirm Your Account Again!");

            }

            return NotFound();
        }
        [HttpPost("ForgetPassword")]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordRequest forgetPasswordRequest)
        {
           
            var user = await _userManager.FindByEmailAsync(forgetPasswordRequest.EmailORUserName) ?? await _userManager.FindByNameAsync(forgetPasswordRequest.EmailORUserName);

            var userOTP = await _userOTPRepository.GetAsync(e => e.ApplicationUserId == user.Id);

            var totalOTPs = userOTP.Count(e => e.Date.Day == DateTime.UtcNow.Day && e.Date.Month == DateTime.UtcNow.Month && e.Date.Year == DateTime.UtcNow.Year);

            if (totalOTPs < 3)
            {
                if (user is not null)
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
                }

                return CreatedAtAction(nameof(ResetPassword), "Accounts", new { area = "Identity", userId = user.Id! }, string.Empty);
            }

            // Send msg
            return BadRequest("Too Many Request, Please try again Later");

        }
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest resetPasswordRequest)
        {
            var userOTP = (await _userOTPRepository.GetAsync(e => e.ApplicationUserId == resetPasswordRequest.UserId)).OrderBy(e => e.Id).LastOrDefault();

            if (userOTP is not null)
            {
                if (DateTime.UtcNow < userOTP.ExpirationDate && !userOTP.Status && userOTP.Code == resetPasswordRequest.Code)
                {
                    return CreatedAtAction(nameof(ChangePassword), "Accounts", new { area = "Identity", userId = userOTP.ApplicationUserId! }, string.Empty);
                }
            }

            // Error
            return BadRequest("Invalid Code");

        }

        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest changePasswordRequest)
        {

            var user = await _userManager.FindByIdAsync(changePasswordRequest.UserId);

            if (user is not null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                await _userManager.ResetPasswordAsync(user, token, changePasswordRequest.Password);

                return Ok("Reset Password Successfully");
            }

            return NotFound();
        }

    }
}