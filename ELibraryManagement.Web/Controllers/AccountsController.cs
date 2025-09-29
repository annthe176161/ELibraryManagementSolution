using ELibraryManagement.Web.Models;
using ELibraryManagement.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ELibraryManagement.Web.Controllers
{
    public class AccountsController : Controller
    {
        private readonly IAuthApiService _authApiService;

        public AccountsController(IAuthApiService authApiService)
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
                TempData["ShowEmailVerificationMessage"] = true;
                return RedirectToAction("Login");
            }

            ModelState.AddModelError("", result.Message);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Login(string? token = null, string? user = null, string? error = null)
        {
            // Debug logging
            Console.WriteLine($"Login GET called with token: {!string.IsNullOrEmpty(token)}, user: {!string.IsNullOrEmpty(user)}, error: {error}");

            // Prevent browser caching to ensure fresh content
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            // Clear any lingering success messages when accessing login page directly
            TempData.Remove("SuccessMessage");

            // Xử lý lỗi từ Google OAuth
            if (!string.IsNullOrEmpty(error))
            {
                TempData["ErrorMessage"] = Uri.UnescapeDataString(error);
                return View();
            }

            // Xử lý callback từ Google OAuth
            if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(user))
            {
                try
                {
                    // Decode user info
                    var userInfo = JsonSerializer.Deserialize<UserViewModel>(Uri.UnescapeDataString(user));

                    if (userInfo != null)
                    {
                        // Lưu token và user info vào session
                        _authApiService.StoreUserSession(token, userInfo);

                        TempData["SuccessMessage"] = "Đăng nhập Google thành công!";

                        // Check if user is admin and redirect appropriately
                        var isAdmin = await _authApiService.IsInRoleAsync("Admin");
                        if (isAdmin)
                        {
                            return RedirectToAction("Index", "Admins");
                        }
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Không thể xử lý thông tin user từ Google.";
                    }
                }
                catch (Exception)
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra trong quá trình đăng nhập Google.";
                }
            }

            if (_authApiService.IsAuthenticated())
            {
                // Check if user is admin and redirect to admin panel
                var isAdmin = await _authApiService.IsInRoleAsync("Admin");
                if (isAdmin)
                {
                    return RedirectToAction("Index", "Admins");
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
                    return RedirectToAction("Index", "Admins");
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

            // Clear any existing success messages to prevent confusion
            TempData.Remove("SuccessMessage");
            TempData.Remove("ErrorMessage");

            // Don't set success message for logout - it's confusing on login page
            return RedirectToAction("Login");
        }

        [HttpGet]
        public async Task<IActionResult> Profile(long? t = null)
        {
            try
            {
                if (!_authApiService.IsAuthenticated())
                {
                    TempData["ErrorMessage"] = "Bạn cần đăng nhập để xem thông tin cá nhân.";
                    return RedirectToAction("Login");
                }

                // Clear Google login success message on Profile page
                if (TempData["SuccessMessage"]?.ToString() == "Đăng nhập Google thành công!")
                {
                    TempData.Remove("SuccessMessage");
                }

                var user = await _authApiService.GetCurrentUserAsync();
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.";
                    _authApiService.Logout();
                    return RedirectToAction("Login");
                }

                // Pass cache-busting timestamp to view
                ViewBag.CacheBuster = t ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                // Debug: Log avatar URL for troubleshooting
                if (TempData["DebugMessage"] != null)
                {
                    TempData["DebugMessage"] += $" | Avatar URL: {user.AvatarUrl} | Cache-buster: {ViewBag.CacheBuster}";
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
            return RedirectToAction("MyBooks", "Books");
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

                // Clear irrelevant success messages (like Google login success) on this page
                TempData.Remove("SuccessMessage");

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
            // Debug: Check ModelState and incoming data
            var debugInfo = $"🔍 DEBUG ModelState: Valid={ModelState.IsValid}";
            if (!ModelState.IsValid)
            {
                debugInfo += " | Errors: " + string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            }

            // Debug: Check Request.Form files
            debugInfo += $" | Request.Form.Files.Count={Request.Form.Files.Count}";
            foreach (var file in Request.Form.Files)
            {
                debugInfo += $" | File: {file.Name}={file.FileName} ({file.Length} bytes)";
            }

            // Debug: Check model.AvatarFile specifically
            debugInfo += $" | model.AvatarFile={(model.AvatarFile != null ? $"'{model.AvatarFile.FileName}' ({model.AvatarFile.Length} bytes)" : "NULL")}";

            TempData["DebugMessage"] = debugInfo;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                bool avatarUploaded = false;
                string successMessage = "";

                // Debug: Check if avatar file exists
                if (model.AvatarFile != null && model.AvatarFile.Length > 0)
                {
                    TempData["DebugMessage"] = $"🔍 DEBUG: Avatar file detected - Name: {model.AvatarFile.FileName}, Size: {model.AvatarFile.Length} bytes, Type: {model.AvatarFile.ContentType}";

                    var uploadResult = await _authApiService.UploadAvatarAsync(model.AvatarFile);
                    if (uploadResult.Success)
                    {
                        TempData["DebugMessage"] += $" | ✅ Upload thành công! Avatar URL từ API: {uploadResult.User?.AvatarUrl ?? "N/A"}";
                        avatarUploaded = true;
                        successMessage = "Cập nhật avatar thành công!";

                        // Update model with new avatar URL to prevent overwrite during profile update
                        model.AvatarUrl = uploadResult.User?.AvatarUrl;
                        TempData["DebugMessage"] += $" | 🔗 Model AvatarUrl updated: {model.AvatarUrl}";

                        // Small delay to ensure database is updated
                        await Task.Delay(500);
                    }
                    else
                    {
                        TempData["ErrorMessage"] = uploadResult.Message;
                        TempData["DebugMessage"] += $" | ❌ Upload lỗi: {uploadResult.Message}";
                        return View(model);
                    }
                }

                // Check if there are profile data changes to update
                bool hasProfileChanges = !string.IsNullOrEmpty(model.FirstName) ||
                                        !string.IsNullOrEmpty(model.LastName) ||
                                        !string.IsNullOrEmpty(model.StudentId) ||
                                        !string.IsNullOrEmpty(model.PhoneNumber) ||
                                        model.DateOfBirth.HasValue ||
                                        !string.IsNullOrEmpty(model.Address);

                if (hasProfileChanges)
                {
                    TempData["DebugMessage"] += " | 🔄 Updating profile info...";

                    // Update profile information (avatar already handled separately)
                    var profileResult = await _authApiService.UpdateProfileAsync(model);

                    if (profileResult.Success)
                    {
                        TempData["DebugMessage"] += " | ✅ Profile update thành công!";

                        if (avatarUploaded)
                        {
                            successMessage = "Cập nhật avatar và thông tin cá nhân thành công!";
                        }
                        else
                        {
                            successMessage = "Cập nhật thông tin cá nhân thành công!";
                        }
                    }
                    else
                    {
                        TempData["DebugMessage"] += $" | ❌ Profile update lỗi: {profileResult.Message}";

                        if (avatarUploaded)
                        {
                            // Avatar uploaded but profile update failed
                            TempData["SuccessMessage"] = "Cập nhật avatar thành công!";
                            TempData["ErrorMessage"] = $"Nhưng không thể cập nhật thông tin cá nhân: {profileResult.Message}";
                        }
                        else
                        {
                            TempData["ErrorMessage"] = profileResult.Message;
                            return View(model);
                        }
                    }
                }
                else if (!avatarUploaded)
                {
                    TempData["DebugMessage"] = "🔍 DEBUG: Không có thay đổi nào để cập nhật";
                    TempData["ErrorMessage"] = "Không có thông tin nào được thay đổi.";
                    return View(model);
                }

                TempData["SuccessMessage"] = successMessage;

                // Add cache-busting parameter to force avatar refresh
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                return RedirectToAction("Profile", new { t = timestamp });
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
                    // Clear ModelState to ensure no validation messages are shown
                    ModelState.Clear();
                    TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
                    TempData["DebugMessage"] = "Redirecting to Profile after successful password change";
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

        [HttpGet]
        public IActionResult EmailConfirmed(bool success, string message)
        {
            ViewBag.Success = success;
            ViewBag.Message = message;
            return View();
        }

        [HttpGet]
        public IActionResult ResendEmailConfirmation()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendEmailConfirmation(ResendEmailConfirmationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var result = await _authApiService.ResendEmailConfirmationAsync(model);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
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
