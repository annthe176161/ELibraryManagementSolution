using ELibraryManagement.Api.DTOs;
using ELibraryManagement.Api.Models;
using ELibraryManagement.Api.Services;
using ELibraryManagement.Api.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ELibraryManagement.Api.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;
        private readonly IEmailService _emailService;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            ILogger<AuthService> logger,
            IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _logger = logger;
            _emailService = emailService;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Email already exists."
                };
            }

            var existingUserName = await _userManager.FindByNameAsync(request.UserName);
            if (existingUserName != null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Username already exists."
                };
            }

            // Create new user
            var user = new ApplicationUser
            {
                UserName = request.UserName,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                StudentId = request.StudentId,
                Address = request.Address,
                PhoneNumber = request.PhoneNumber,
                DateOfBirth = request.DateOfBirth,
                CreatedAt = DateTime.UtcNow,
                EmailConfirmed = false // Email needs to be confirmed
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = string.Join(", ", result.Errors.Select(e => e.Description))
                };
            }

            // Assign default role
            await _userManager.AddToRoleAsync(user, "User");

            // Generate email confirmation token
            var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = $"{_configuration["AppSettings:BaseUrl"]}/api/auth/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(emailToken)}";

            // Send confirmation email
            await _emailService.SendEmailConfirmationAsync(user.Email, confirmationLink);

            return new AuthResponseDto
            {
                Success = true,
                Message = "User registered successfully. Please check your email to confirm your account before logging in.",
                Token = null, // Don't provide token until email is confirmed
                Expiration = null,
                User = await GetUserDtoAsync(user)
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            var user = await _userManager.FindByEmailAsync(request.UserNameOrEmail) ??
                      await _userManager.FindByNameAsync(request.UserNameOrEmail);

            if (user == null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid username/email or password."
                };
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);

            if (!result.Succeeded)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid username/email or password."
                };
            }

            // Check if email is confirmed
            if (!user.EmailConfirmed)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Please confirm your email address before logging in. Check your email for the confirmation link."
                };
            }

            // Generate JWT token
            var token = await GenerateJwtTokenAsync(user);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Login successful.",
                Token = token,
                Expiration = DateTime.UtcNow.AddHours(24),
                User = await GetUserDtoAsync(user)
            };
        }

        public async Task<AuthResponseDto> GetCurrentUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            return new AuthResponseDto
            {
                Success = true,
                Message = "User retrieved successfully.",
                User = await GetUserDtoAsync(user)
            };
        }

        public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequestDto request)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            return result.Succeeded;
        }

        public async Task<bool> AssignRoleAsync(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            // Ensure role exists
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }

            var result = await _userManager.AddToRoleAsync(user, roleName);
            return result.Succeeded;
        }

        public async Task<IList<string>> GetUserRolesAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new List<string>();
            }

            return await _userManager.GetRolesAsync(user);
        }

        private async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? "")
            };

            // Add roles to claims
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "default-secret-key-for-development-only"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"] ?? "ELibraryManagement.Api",
                audience: _configuration["Jwt:Audience"] ?? "ELibraryManagement.Client",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<UserDto> GetUserDtoAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            return new UserDto
            {
                Id = user.Id,
                UserName = user.UserName ?? "",
                Email = user.Email ?? "",
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                StudentId = user.StudentId,
                AvatarUrl = user.AvatarUrl,
                DateOfBirth = user.DateOfBirth,
                CreatedAt = user.CreatedAt,
                Roles = roles.ToList()
            };
        }

        public async Task<AuthResponseDto> UpdateProfileAsync(string userId, UpdateProfileRequestDto request)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "User not found."
                    };
                }

                // Cập nhật thông tin
                user.FirstName = request.FirstName;
                user.LastName = request.LastName;
                user.StudentId = request.StudentId;
                user.PhoneNumber = request.PhoneNumber;
                user.DateOfBirth = request.DateOfBirth;
                user.Address = request.Address;
                user.AvatarUrl = request.AvatarUrl;
                user.UpdatedAt = DateTime.UtcNow;

                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Failed to update profile: " + string.Join(", ", result.Errors.Select(e => e.Description))
                    };
                }

                // Trả về thông tin user mới
                var userDto = await GetUserDtoAsync(user);

                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Profile updated successfully!",
                    User = userDto
                };
            }
            catch (Exception ex)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }

        public async Task<AuthResponseDto> ForgotPasswordAsync(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    // Không tiết lộ thông tin user có tồn tại hay không
                    return new AuthResponseDto
                    {
                        Success = true,
                        Message = "Nếu email tồn tại trong hệ thống, chúng tôi đã gửi link reset mật khẩu đến email của bạn."
                    };
                }

                // Tạo password reset token
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                // TODO: Gửi email với link reset password
                // Trong môi trường production, bạn sẽ cần implement email service
                // Ví dụ: await _emailService.SendPasswordResetEmailAsync(user.Email, token);

                // Lưu token vào log để test (chỉ dùng trong development)
                _logger.LogInformation($"Password reset token for {email}: {token}");

                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Nếu email tồn tại trong hệ thống, chúng tôi đã gửi link reset mật khẩu đến email của bạn."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ForgotPasswordAsync");
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi xử lý yêu cầu."
                };
            }
        }

        public async Task<AuthResponseDto> ResetPasswordAsync(string email, string token, string newPassword)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Email không tồn tại trong hệ thống."
                    };
                }

                var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

                if (!result.Succeeded)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Token không hợp lệ hoặc đã hết hạn. " + string.Join(", ", result.Errors.Select(e => e.Description))
                    };
                }

                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Mật khẩu đã được reset thành công. Bạn có thể đăng nhập bằng mật khẩu mới."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ResetPasswordAsync");
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi reset mật khẩu."
                };
            }
        }

        public async Task<AuthResponseDto> HandleGoogleLoginAsync(ExternalLoginInfo info)
        {
            try
            {
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                var firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName);
                var lastName = info.Principal.FindFirstValue(ClaimTypes.Surname);

                if (string.IsNullOrEmpty(email))
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Không thể lấy email từ Google."
                    };
                }

                // Tìm user theo email
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    // Tạo user mới
                    user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        FirstName = firstName ?? "",
                        LastName = lastName ?? "",
                        EmailConfirmed = true, // Google đã xác thực email
                        StudentId = GenerateRandomStudentId() // Tạo student ID ngẫu nhiên
                    };

                    var result = await _userManager.CreateAsync(user);
                    if (!result.Succeeded)
                    {
                        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                        return new AuthResponseDto
                        {
                            Success = false,
                            Message = $"Tạo tài khoản thất bại: {errors}"
                        };
                    }

                    // Thêm role User mặc định
                    await _userManager.AddToRoleAsync(user, "User");
                }

                // Liên kết external login với user
                var existingLogin = await _userManager.GetLoginsAsync(user);
                if (!existingLogin.Any(x => x.LoginProvider == info.LoginProvider))
                {
                    await _userManager.AddLoginAsync(user, info);
                }

                // Tạo JWT token
                var token = await GenerateJwtTokenAsync(user);
                var userDto = await GetUserDtoAsync(user);

                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Đăng nhập Google thành công",
                    Token = token,
                    User = userDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in HandleGoogleLoginAsync");
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi đăng nhập bằng Google."
                };
            }
        }

        public async Task<AuthResponseDto> ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Email confirmed successfully. You can now log in to your account."
                };
            }

            return new AuthResponseDto
            {
                Success = false,
                Message = "Email confirmation failed. The token may be invalid or expired."
            };
        }

        public async Task<AuthResponseDto> ResendEmailConfirmationAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            if (user.EmailConfirmed)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Email is already confirmed."
                };
            }

            // Generate new email confirmation token
            var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = $"{_configuration["AppSettings:BaseUrl"]}/api/auth/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(emailToken)}";

            // Send confirmation email
            var emailSent = await _emailService.SendEmailConfirmationAsync(user.Email, confirmationLink);

            if (emailSent)
            {
                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Confirmation email sent successfully. Please check your email."
                };
            }

            return new AuthResponseDto
            {
                Success = false,
                Message = "Failed to send confirmation email. Please try again later."
            };
        }

        private string GenerateRandomStudentId()
        {
            // Tạo student ID ngẫu nhiên với format: SV + 8 số
            var random = new Random();
            var studentId = "SV" + random.Next(10000000, 99999999);
            return studentId;
        }
    }
}
