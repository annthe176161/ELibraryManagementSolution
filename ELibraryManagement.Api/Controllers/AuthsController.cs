using ELibraryManagement.Api.DTOs;
using ELibraryManagement.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;

namespace ELibraryManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthsController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly SignInManager<Models.ApplicationUser> _signInManager;

        public AuthsController(IAuthService authService, SignInManager<Models.ApplicationUser> signInManager)
        {
            _authService = authService;
            _signInManager = signInManager;
        }

        /// <summary>
        /// Đăng ký tài khoản mới
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.RegisterAsync(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Đăng nhập
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.LoginAsync(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Lấy thông tin user hiện tại
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _authService.GetCurrentUserAsync(userId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Đổi mật khẩu
        /// </summary>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Success = false, Message = "Dữ liệu không hợp lệ." });
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { Success = false, Message = "Không thể xác thực người dùng." });
            }

            var result = await _authService.ChangePasswordAsync(userId, request);

            if (!result)
            {
                return BadRequest(new { Success = false, Message = "Không thể đổi mật khẩu. Vui lòng kiểm tra mật khẩu hiện tại." });
            }

            return Ok(new { Success = true, Message = "Đổi mật khẩu thành công!" });
        }

        /// <summary>
        /// Lấy danh sách roles của user hiện tại
        /// </summary>
        [HttpGet("roles")]
        [Authorize]
        public async Task<IActionResult> GetUserRoles()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var roles = await _authService.GetUserRolesAsync(userId);
            return Ok(new { Roles = roles });
        }

        /// <summary>
        /// Gán role cho user (chỉ dành cho Admin)
        /// </summary>
        [HttpPost("assign-role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.AssignRoleAsync(request.UserId, request.RoleName);

            if (!result)
            {
                return BadRequest(new { Message = "Không thể gán vai trò." });
            }

            return Ok(new { Message = $"Đã gán vai trò '{request.RoleName}' thành công cho người dùng." });
        }

        /// <summary>
        /// Cập nhật thông tin cá nhân
        /// </summary>
        [HttpPut("update-profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _authService.UpdateProfileAsync(userId, request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Quên mật khẩu - Gửi email reset password
        /// </summary>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.ForgotPasswordAsync(request.Email);

            // Luôn trả về success để tránh việc kẻ tấn công biết email có tồn tại hay không
            return Ok(new
            {
                Success = true,
                Message = "Chúng tôi đã gửi hướng dẫn đặt lại mật khẩu đến địa chỉ email của bạn. Vui lòng kiểm tra hộp thư (bao gồm cả thư mục spam)."
            });
        }

        /// <summary>
        /// Reset mật khẩu
        /// </summary>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Đăng nhập bằng Google
        /// </summary>
        [HttpGet("google-login")]
        [AllowAnonymous]
        public IActionResult GoogleLogin()
        {
            // Sử dụng absolute URL để đảm bảo Google redirect đúng
            var redirectUrl = "https://localhost:7125/api/auths/google-response";
            var properties = _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);
            return Challenge(properties, "Google");
        }

        /// <summary>
        /// Xử lý callback từ Google
        /// </summary>
        [HttpGet("google-response")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleResponse()
        {
            Console.WriteLine("GoogleResponse callback được gọi");

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                Console.WriteLine("ExternalLoginInfo null - Google callback failed");
                // Redirect về login page với error message
                return Redirect($"https://localhost:7208/Accounts/Login?error={Uri.EscapeDataString("Lỗi đăng nhập bên ngoài")}");
            }

            Console.WriteLine($"Google login info received: {info.Principal?.Identity?.Name}");

            var result = await _authService.HandleGoogleLoginAsync(info);
            if (!result.Success)
            {
                // Redirect về login page với error message
                return Redirect($"https://localhost:7208/Accounts/Login?error={Uri.EscapeDataString(result.Message)}");
            }

            // Redirect về frontend với token
            var userData = JsonSerializer.Serialize(result.User);
            return Redirect($"https://localhost:7208/Accounts/Login?token={result.Token}&user={Uri.EscapeDataString(userData)}");
        }

        /// <summary>
        /// Xác nhận email
        /// </summary>
        [HttpGet("confirm-email")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return BadRequest("Yêu cầu xác nhận email không hợp lệ.");
            }

            var result = await _authService.ConfirmEmailAsync(userId, token);

            if (result.Success)
            {
                // Redirect to frontend with success message
                return Redirect($"https://localhost:7208/Accounts/EmailConfirmed?success=true&message={Uri.EscapeDataString(result.Message)}");
            }

            // Redirect to frontend with error message
            return Redirect($"https://localhost:7208/Accounts/EmailConfirmed?success=false&message={Uri.EscapeDataString(result.Message)}");
        }

        /// <summary>
        /// Gửi lại email xác nhận
        /// </summary>
        [HttpPost("resend-email-confirmation")]
        [AllowAnonymous]
        public async Task<IActionResult> ResendEmailConfirmation([FromBody] ResendEmailConfirmationDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.ResendEmailConfirmationAsync(request.Email);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }

    public class AssignRoleRequestDto
    {
        public string UserId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
    }

    public class ResendEmailConfirmationDto
    {
        public string Email { get; set; } = string.Empty;
    }
}
