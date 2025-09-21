using ELibraryManagement.Api.DTOs;
using ELibraryManagement.Api.Services.Interfaces;
using ELibraryManagement.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ELibraryManagement.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;

namespace ELibraryManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class UserController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly ApplicationDbContext _context;

        public UserController(UserManager<ApplicationUser> userManager, ICloudinaryService cloudinaryService, ApplicationDbContext context)
        {
            _userManager = userManager;
            _cloudinaryService = cloudinaryService;
            _context = context;
        }

        /// <summary>
        /// Lấy danh sách tất cả người dùng (chỉ dành cho Admin)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var userDtos = new List<UserDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                // Calculate ActiveBorrows and TotalBorrows for each user
                // Treat Overdue as active so admin shows overdue items under "Đang mượn"
                // Treat Overdue, Lost and Damaged as active so admin shows them under "Đang mượn"
                var activeBorrowsCount = await _context.BorrowRecords
                    .Where(br => br.UserId == user.Id && (
                        br.Status == BorrowStatus.Borrowed ||
                        br.Status == BorrowStatus.Overdue ||
                        br.Status == BorrowStatus.Lost ||
                        br.Status == BorrowStatus.Damaged))
                    .CountAsync();

                // Count total borrows for the user. Include Lost and Damaged in total so admin statistics
                // reflect all completed/consumed borrow transactions (Borrowed, Returned, Overdue, Lost, Damaged).
                var totalBorrowsCount = await _context.BorrowRecords
                    .Where(br => br.UserId == user.Id && (
                        br.Status == BorrowStatus.Borrowed ||
                        br.Status == BorrowStatus.Returned ||
                        br.Status == BorrowStatus.Overdue ||
                        br.Status == BorrowStatus.Lost ||
                        br.Status == BorrowStatus.Damaged))
                    .CountAsync();

                userDtos.Add(new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    StudentId = user.StudentId,
                    PhoneNumber = user.PhoneNumber,
                    Address = user.Address,
                    DateOfBirth = user.DateOfBirth,
                    CreatedAt = user.CreatedAt,
                    IsActive = user.IsActive,
                    LastLoginDate = user.LastLoginDate,
                    Roles = roles.ToList(),
                    TotalBorrows = totalBorrowsCount,
                    ActiveBorrows = activeBorrowsCount
                });
            }

            return Ok(userDtos);
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
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "No file provided"
                    });
                }

                var userId = User.Identity?.Name;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new
                    {
                        Success = false,
                        Message = "Không tìm thấy người dùng"
                    });
                }

                var user = await _userManager.FindByNameAsync(userId);
                if (user == null)
                {
                    return NotFound(new
                    {
                        Success = false,
                        Message = "Không tìm thấy người dùng"
                    });
                }

                // Upload ảnh lên Cloudinary
                var imageUrl = await _cloudinaryService.UploadImageAsync(file, "avatars");
                if (imageUrl == null)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Không thể tải lên hình ảnh"
                    });
                }

                // Delete old avatar from Cloudinary if exists
                if (!string.IsNullOrEmpty(user.AvatarUrl))
                {
                    try
                    {
                        // Extract public ID from old URL
                        var uri = new Uri(user.AvatarUrl);
                        var pathSegments = uri.AbsolutePath.Split('/');
                        var fileNameWithExtension = pathSegments.Last();
                        var publicId = $"elibrary/avatars/{Path.GetFileNameWithoutExtension(fileNameWithExtension)}";
                        await _cloudinaryService.DeleteImageAsync(publicId);
                    }
                    catch (Exception)
                    {
                        // Log but don't fail the upload if deletion fails
                        // (old image will remain on Cloudinary but that's acceptable)
                    }
                }

                // Cập nhật AvatarUrl trong database
                user.AvatarUrl = imageUrl;
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    return Ok(new
                    {
                        Success = true,
                        Message = "Tải lên avatar thành công",
                        AvatarUrl = imageUrl
                    });
                }

                return BadRequest(new
                {
                    Success = false,
                    Message = "Không thể cập nhật avatar người dùng"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"Internal server error: {ex.Message}"
                });
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

                // Calculate ActiveBorrows - treat Borrowed, Overdue, Lost and Damaged as active for admin display
                var activeBorrowsCount = await _context.BorrowRecords
                    .Where(br => br.UserId == user.Id && (
                        br.Status == BorrowStatus.Borrowed ||
                        br.Status == BorrowStatus.Overdue ||
                        br.Status == BorrowStatus.Lost ||
                        br.Status == BorrowStatus.Damaged))
                    .CountAsync();

                // Calculate TotalBorrows - include Borrowed, Returned, Overdue, Lost and Damaged
                var totalBorrowsCount = await _context.BorrowRecords
                    .Where(br => br.UserId == user.Id && (
                        br.Status == BorrowStatus.Borrowed ||
                        br.Status == BorrowStatus.Returned ||
                        br.Status == BorrowStatus.Overdue ||
                        br.Status == BorrowStatus.Lost ||
                        br.Status == BorrowStatus.Damaged))
                    .CountAsync();

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
                    IsActive = user.IsActive,
                    Roles = roles.ToList(),
                    TotalBorrows = totalBorrowsCount,
                    ActiveBorrows = activeBorrowsCount
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

                // Set both IsActive and LockoutEnd for consistency
                user.IsActive = false;
                user.UpdatedAt = DateTime.UtcNow;

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    return BadRequest("Không thể cập nhật thông tin người dùng");
                }

                // Also set lockout for additional security
                var lockoutResult = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));

                if (lockoutResult.Succeeded)
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

                // Set both IsActive and remove lockout for consistency
                user.IsActive = true;
                user.UpdatedAt = DateTime.UtcNow;

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    return BadRequest("Không thể cập nhật thông tin người dùng");
                }

                // Remove lockout to enable user
                var lockoutResult = await _userManager.SetLockoutEndDateAsync(user, null);

                if (lockoutResult.Succeeded)
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
