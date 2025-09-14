using ELibraryManagement.Api.DTOs;
using ELibraryManagement.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ELibraryManagement.Api.Models;

namespace ELibraryManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class UserController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICloudinaryService _cloudinaryService;

        public UserController(UserManager<ApplicationUser> userManager, ICloudinaryService cloudinaryService)
        {
            _userManager = userManager;
            _cloudinaryService = cloudinaryService;
        }

        /// <summary>
        /// Tạo user mẫu để test (chỉ dành cho Admin)
        /// </summary>
        [HttpPost("create-test-user")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateTestUser()
        {
            var testUser = new ApplicationUser
            {
                UserName = "testuser",
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                PhoneNumber = "0123456789",
                Address = "123 Test Street",
                DateOfBirth = new DateTime(1990, 1, 1),
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(testUser, "Test@123");

            if (result.Succeeded)
            {
                return Ok(new
                {
                    Message = "Tạo người dùng test thành công",
                    UserId = testUser.Id,
                    UserName = testUser.UserName,
                    Email = testUser.Email,
                    Password = "Test@123"
                });
            }

            return BadRequest(result.Errors);
        }

        /// <summary>
        /// Lấy thông tin user theo ID
        /// </summary>
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound("Không tìm thấy người dùng");
            }

            return Ok(new
            {
                user.Id,
                user.UserName,
                user.Email,
                FullName = $"{user.FirstName} {user.LastName}".Trim(),
                user.PhoneNumber,
                user.Address,
                user.DateOfBirth
            });
        }

        /// <summary>
        /// Lấy danh sách tất cả users với thông tin đầy đủ cho admin
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = new List<object>();

            foreach (var user in _userManager.Users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                users.Add(new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.FirstName,
                    user.LastName,
                    FullName = $"{user.FirstName} {user.LastName}".Trim(),
                    user.PhoneNumber,
                    user.Address,
                    user.DateOfBirth,
                    user.AvatarUrl,
                    user.StudentId,
                    user.CreatedAt,
                    user.LockoutEnd,
                    IsActive = user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow,
                    Roles = roles.ToList(),
                    TotalBorrows = 0, // TODO: Calculate from borrow records
                    ActiveBorrows = 0 // TODO: Calculate from active borrow records
                });
            }

            return Ok(users);
        }

        /// <summary>
        /// Upload avatar cho user
        /// </summary>
        [HttpPost("upload-avatar")]
        [Authorize]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file provided");
                }

                var userId = User.Identity?.Name;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Không tìm thấy người dùng");
                }

                var user = await _userManager.FindByNameAsync(userId);
                if (user == null)
                {
                    return NotFound("Không tìm thấy người dùng");
                }

                // Upload ảnh lên Cloudinary
                var imageUrl = await _cloudinaryService.UploadImageAsync(file, "avatars");
                if (imageUrl == null)
                {
                    return BadRequest("Không thể tải lên hình ảnh");
                }

                // Cập nhật AvatarUrl trong database
                user.AvatarUrl = imageUrl;
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    return Ok(new
                    {
                        Message = "Tải lên avatar thành công",
                        AvatarUrl = imageUrl
                    });
                }

                return BadRequest("Không thể cập nhật avatar người dùng");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Upload avatar cho user theo ID (Admin only)
        /// </summary>
        [HttpPost("upload-avatar/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadAvatarForUser(string userId, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file provided");
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound("Không tìm thấy người dùng");
                }

                // Upload ảnh lên Cloudinary
                var imageUrl = await _cloudinaryService.UploadImageAsync(file, "avatars");
                if (imageUrl == null)
                {
                    return BadRequest("Không thể tải lên hình ảnh");
                }

                // Cập nhật AvatarUrl trong database
                user.AvatarUrl = imageUrl;
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    return Ok(new
                    {
                        Message = "Tải lên avatar thành công",
                        AvatarUrl = imageUrl,
                        UserId = userId
                    });
                }

                return BadRequest("Không thể cập nhật avatar người dùng");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết user theo ID cho admin
        /// </summary>
        [HttpGet("admin/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserById(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound("Không tìm thấy người dùng");
                }

                var roles = await _userManager.GetRolesAsync(user);

                var result = new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.FirstName,
                    user.LastName,
                    FullName = $"{user.FirstName} {user.LastName}".Trim(),
                    user.PhoneNumber,
                    user.Address,
                    user.DateOfBirth,
                    user.AvatarUrl,
                    user.StudentId,
                    user.CreatedAt,
                    user.LockoutEnd,
                    IsActive = user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow,
                    Roles = roles.ToList(),
                    TotalBorrows = 0, // TODO: Calculate from borrow records
                    ActiveBorrows = 0 // TODO: Calculate from active borrow records
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Vô hiệu hóa user
        /// </summary>
        [HttpPost("{id}/disable")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DisableUser(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound("Không tìm thấy người dùng");
                }

                // Set lockout end to a far future date to disable user
                var result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));

                if (result.Succeeded)
                {
                    return Ok(new { message = "Đã vô hiệu hóa người dùng thành công" });
                }

                return BadRequest("Không thể vô hiệu hóa người dùng");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Kích hoạt user
        /// </summary>
        [HttpPost("{id}/enable")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EnableUser(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound("Không tìm thấy người dùng");
                }

                // Remove lockout to enable user
                var result = await _userManager.SetLockoutEndDateAsync(user, null);

                if (result.Succeeded)
                {
                    return Ok(new { message = "Đã kích hoạt người dùng thành công" });
                }

                return BadRequest("Không thể kích hoạt người dùng");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
