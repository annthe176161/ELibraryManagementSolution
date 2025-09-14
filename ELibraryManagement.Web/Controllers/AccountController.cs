using ELibraryManagement.Web.Models;
using ELibraryManagement.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace ELibraryManagement.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthApiService _authApiService;

        public AccountController(IAuthApiService authApiService)
        {
            _authApiService = authApiService;
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (_authApiService.IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _authApiService.RegisterAsync(model);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction("Login");
            }

            ModelState.AddModelError("", result.Message);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            if (_authApiService.IsAuthenticated())
            {
                // Check if user is admin and redirect to admin panel
                var isAdmin = await _authApiService.IsInRoleAsync("Admin");
                if (isAdmin)
                {
                    return RedirectToAction("Index", "Admin");
                }
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _authApiService.LoginAsync(model);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;

                // Check if user is admin and redirect to admin panel
                var isAdmin = await _authApiService.IsInRoleAsync("Admin");
                if (isAdmin)
                {
                    return RedirectToAction("Index", "Admin");
                }

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", result.Message);
            return View(model);
        }

        [HttpPost]
        public IActionResult Logout()
        {
            _authApiService.Logout();
            TempData["SuccessMessage"] = "Đăng xuất thành công!";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            try
            {
                if (!_authApiService.IsAuthenticated())
                {
                    TempData["ErrorMessage"] = "Bạn cần đăng nhập để xem thông tin cá nhân.";
                    return RedirectToAction("Login");
                }

                var user = await _authApiService.GetCurrentUserAsync();
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.";
                    _authApiService.Logout();
                    return RedirectToAction("Login");
                }

                return View(user);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra khi tải thông tin cá nhân: {ex.Message}";
                return RedirectToAction("Login");
            }
        }

        // Redirect action for backward compatibility
        public IActionResult MyBooks()
        {
            return RedirectToAction("MyBooks", "Book");
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            try
            {
                if (!_authApiService.IsAuthenticated())
                {
                    TempData["ErrorMessage"] = "Bạn cần đăng nhập để chỉnh sửa thông tin.";
                    return RedirectToAction("Login");
                }

                var user = await _authApiService.GetCurrentUserAsync();
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.";
                    _authApiService.Logout();
                    return RedirectToAction("Login");
                }

                var model = new EditProfileViewModel
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    StudentId = user.StudentId,
                    PhoneNumber = user.PhoneNumber,
                    DateOfBirth = user.DateOfBirth,
                    Address = user.Address,
                    AvatarUrl = user.AvatarUrl
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
                return RedirectToAction("Profile");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Nếu có file avatar được upload, upload lên Cloudinary trước
                if (model.AvatarFile != null && model.AvatarFile.Length > 0)
                {
                    var uploadResult = await _authApiService.UploadAvatarAsync(model.AvatarFile);
                    if (uploadResult.Success)
                    {
                        // Nếu upload thành công, cập nhật AvatarUrl
                        TempData["SuccessMessage"] = "Upload avatar thành công!";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = uploadResult.Message;
                        return View(model);
                    }
                }

                // Cập nhật thông tin profile
                var result = await _authApiService.UpdateProfileAsync(model);

                if (result.Success)
                {
                    if (string.IsNullOrEmpty(TempData["SuccessMessage"]?.ToString()))
                    {
                        TempData["SuccessMessage"] = result.Message;
                    }
                    return RedirectToAction("Profile");
                }

                TempData["ErrorMessage"] = result.Message;
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            if (!_authApiService.IsAuthenticated())
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để đổi mật khẩu.";
                return RedirectToAction("Login");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var result = await _authApiService.ChangePasswordAsync(model);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction("Profile");
                }

                TempData["ErrorMessage"] = result.Message;
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var result = await _authApiService.ForgotPasswordAsync(model);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return View(model);
                }

                TempData["ErrorMessage"] = result.Message;
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Link reset mật khẩu không hợp lệ.";
                return RedirectToAction("ForgotPassword");
            }

            var model = new ResetPasswordViewModel
            {
                Email = email,
                Token = token
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var result = await _authApiService.ResetPasswordAsync(model);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = "Mật khẩu đã được reset thành công. Bạn có thể đăng nhập bằng mật khẩu mới.";
                    return RedirectToAction("Login");
                }

                TempData["ErrorMessage"] = result.Message;
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
                return View(model);
            }
        }
    }
}
