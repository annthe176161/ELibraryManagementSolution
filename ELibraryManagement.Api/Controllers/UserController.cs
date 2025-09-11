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

        public UserController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
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
                    Message = "Test user created successfully",
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
                return NotFound("User not found");
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
        /// Lấy danh sách tất cả users
        /// </summary>
        [HttpGet]
        public IActionResult GetAllUsers()
        {
            var users = _userManager.Users.Select(u => new
            {
                u.Id,
                u.UserName,
                u.Email,
                FullName = $"{u.FirstName} {u.LastName}".Trim(),
                u.PhoneNumber,
                u.Address,
                u.DateOfBirth
            }).ToList();

            return Ok(users);
        }
    }
}
