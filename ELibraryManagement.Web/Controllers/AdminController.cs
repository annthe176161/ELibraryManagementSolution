using ELibraryManagement.Web.Models;
using ELibraryManagement.Web.Models.DTOs;
using ELibraryManagement.Web.Models.DTOs.CategoryDtos;
using ELibraryManagement.Web.Services;
using ELibraryManagement.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using ViewModels = ELibraryManagement.Web.Models.ViewModels;

namespace ELibraryManagement.Web.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAuthApiService _authApiService;
        private readonly IReviewApiService _reviewApiService;
        private readonly IBorrowApiService _borrowApiService;
        private readonly ICategoryApiService _categoryApiService;
        private readonly IFineApiService _fineApiService;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            IAuthApiService authApiService,
            IReviewApiService reviewApiService,
            IBorrowApiService borrowApiService,
            ICategoryApiService categoryApiService,
            IFineApiService fineApiService,
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<AdminController> logger)
        {
            _authApiService = authApiService;
            _reviewApiService = reviewApiService;
            _borrowApiService = borrowApiService;
            _categoryApiService = categoryApiService;
            _fineApiService = fineApiService;
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        private string GetApiBaseUrl()
        {
            var httpsUrl = _configuration["ApiSettings:BaseUrl"];
            var httpUrl = _configuration["ApiSettings:BaseUrlHttp"];
            return httpsUrl ?? httpUrl ?? "https://localhost:7125";
        }

        // Helper method to check if user is admin
        private async Task<bool> IsAdminAsync()
        {
            if (!_authApiService.IsAuthenticated())
                return false;

            return await _authApiService.IsInRoleAsync("Admin");
        }

        private async Task<IActionResult?> CheckAdminAccessAsync()
        {
            if (!_authApiService.IsAuthenticated())
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để truy cập trang này.";
                return RedirectToAction("Login", "Account");
            }

            if (!await IsAdminAsync())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang quản trị.";
                return RedirectToAction("Index", "Home");
            }

            return null;
        }

        // GET: Admin Dashboard
        public async Task<IActionResult> Index()
        {
            var accessCheck = await CheckAdminAccessAsync();
            if (accessCheck != null) return accessCheck;

            try
            {
                var token = _authApiService.GetCurrentToken();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // Get dashboard statistics
                var dashboardData = new AdminDashboardViewModel();

                // Get total users count (all users)
                try
                {
                    var usersResponse = await _httpClient.GetAsync($"{GetApiBaseUrl()}/api/User");
                    if (usersResponse.IsSuccessStatusCode)
                    {
                        var usersContent = await usersResponse.Content.ReadAsStringAsync();
                        var users = JsonSerializer.Deserialize<List<AdminUserViewModel>>(usersContent, _jsonOptions);
                        dashboardData.TotalUsers = users?.Count ?? 0;
                        // Count only students (exclude admins)
                        dashboardData.TotalStudents = users?.Count(u => !u.Roles.Contains("Admin")) ?? 0;
                    }
                    else
                    {
                        dashboardData.TotalUsers = 0;
                        dashboardData.TotalStudents = 0;
                    }
                }
                catch (Exception)
                {
                    dashboardData.TotalUsers = 0;
                    dashboardData.TotalStudents = 0;
                }

                // Get total books count
                try
                {
                    var booksResponse = await _httpClient.GetAsync($"{GetApiBaseUrl()}/api/Book/admin/all");
                    if (booksResponse.IsSuccessStatusCode)
                    {
                        var booksContent = await booksResponse.Content.ReadAsStringAsync();
                        var books = JsonSerializer.Deserialize<List<object>>(booksContent, _jsonOptions);
                        dashboardData.TotalBooks = books?.Count ?? 0;
                    }
                    else
                    {
                        dashboardData.TotalBooks = 0;
                    }
                }
                catch (Exception)
                {
                    dashboardData.TotalBooks = 0;
                }

                // Get borrow records statistics
                try
                {
                    var borrowsResponse = await _httpClient.GetAsync($"{GetApiBaseUrl()}/api/Borrow/admin/all");
                    if (borrowsResponse.IsSuccessStatusCode)
                    {
                        var borrowsContent = await borrowsResponse.Content.ReadAsStringAsync();
                        var borrows = JsonSerializer.Deserialize<List<BorrowRecordViewModel>>(borrowsContent, _jsonOptions);

                        if (borrows != null)
                        {
                            dashboardData.TotalBorrows = borrows.Count;
                            dashboardData.ActiveBorrows = borrows.Count(b => b.Status == "Borrowed");
                            dashboardData.RequestedBorrows = borrows.Count(b => b.Status == "Requested");
                            dashboardData.ReturnedBorrows = borrows.Count(b => b.Status == "Returned");
                            dashboardData.CancelledBorrows = borrows.Count(b => b.Status == "Cancelled");
                            dashboardData.OverdueBorrows = borrows.Count(b => b.IsOverdue);
                        }
                        else
                        {
                            dashboardData.TotalBorrows = 0;
                            dashboardData.ActiveBorrows = 0;
                            dashboardData.RequestedBorrows = 0;
                            dashboardData.ReturnedBorrows = 0;
                            dashboardData.CancelledBorrows = 0;
                            dashboardData.OverdueBorrows = 0;
                        }
                    }
                    else
                    {
                        dashboardData.TotalBorrows = 0;
                        dashboardData.ActiveBorrows = 0;
                        dashboardData.RequestedBorrows = 0;
                        dashboardData.ReturnedBorrows = 0;
                        dashboardData.CancelledBorrows = 0;
                        dashboardData.OverdueBorrows = 0;
                    }
                }
                catch (Exception)
                {
                    dashboardData.TotalBorrows = 0;
                    dashboardData.ActiveBorrows = 0;
                    dashboardData.RequestedBorrows = 0;
                    dashboardData.ReturnedBorrows = 0;
                    dashboardData.CancelledBorrows = 0;
                    dashboardData.OverdueBorrows = 0;
                }

                // Get total reviews count
                try
                {
                    var reviewsResponse = await _httpClient.GetAsync($"{GetApiBaseUrl()}/api/Review/all");
                    if (reviewsResponse.IsSuccessStatusCode)
                    {
                        var reviewsContent = await reviewsResponse.Content.ReadAsStringAsync();
                        var reviews = JsonSerializer.Deserialize<List<object>>(reviewsContent, _jsonOptions);
                        dashboardData.TotalReviews = reviews?.Count ?? 0;
                    }
                    else
                    {
                        dashboardData.TotalReviews = 0;
                    }
                }
                catch (Exception)
                {
                    dashboardData.TotalReviews = 0;
                }

                return View(dashboardData);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra khi tải trang quản trị: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Admin/Users - User Management
        public async Task<IActionResult> Users()
        {
            var accessCheck = await CheckAdminAccessAsync();
            if (accessCheck != null) return accessCheck;

            try
            {
                var token = _authApiService.GetCurrentToken();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync($"{GetApiBaseUrl()}/api/User");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var users = JsonSerializer.Deserialize<List<AdminUserViewModel>>(content, _jsonOptions);
                    return View(users ?? new List<AdminUserViewModel>());
                }

                TempData["ErrorMessage"] = "Không thể tải danh sách người dùng.";
                return View(new List<AdminUserViewModel>());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
                return View(new List<AdminUserViewModel>());
            }
        }

        // GET: Admin/Reviews - Review Management
        public async Task<IActionResult> Reviews()
        {
            var accessCheck = await CheckAdminAccessAsync();
            if (accessCheck != null) return accessCheck;

            try
            {
                var token = _authApiService.GetCurrentToken();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync($"{GetApiBaseUrl()}/api/Review/all");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var reviews = JsonSerializer.Deserialize<List<ReviewViewModel>>(content, _jsonOptions);
                    return View(reviews ?? new List<ReviewViewModel>());
                }

                TempData["ErrorMessage"] = "Không thể tải danh sách đánh giá.";
                return View(new List<ReviewViewModel>());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
                return View(new List<ReviewViewModel>());
            }
        }

        // POST: Admin/DeleteReview - Delete Review
        [HttpPost]
        public async Task<IActionResult> DeleteReview(int reviewId)
        {
            var accessCheck = await CheckAdminAccessAsync();
            if (accessCheck != null) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var token = _authApiService.GetCurrentToken();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.DeleteAsync($"{GetApiBaseUrl()}/api/Review/{reviewId}");

                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true, message = "Đánh giá đã được xóa thành công!" });
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return Json(new { success = false, message = "Không thể xóa đánh giá: " + errorContent });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        // GET: Admin/GetReviewDetail - Get Review Detail
        [HttpGet]
        public async Task<IActionResult> GetReviewDetail(int id)
        {
            var accessCheck = await CheckAdminAccessAsync();
            if (accessCheck != null) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var token = _authApiService.GetCurrentToken();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync($"{GetApiBaseUrl()}/api/Review/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var review = JsonSerializer.Deserialize<ReviewDetailViewModel>(content, _jsonOptions);
                    return Json(new { success = true, data = review });
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return Json(new { success = false, message = "Không thể tải chi tiết đánh giá: " + errorContent });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        // GET: Admin/Categories - Category Management
        public async Task<IActionResult> Categories()
        {
            var accessCheck = await CheckAdminAccessAsync();
            if (accessCheck != null) return accessCheck;

            try
            {
                var result = await _categoryApiService.GetAllCategoriesAsync(includeInactive: true);

                if (result.Success && result.Categories != null)
                {
                    var categories = result.Categories.Select(c => new ViewModels.CategoryViewModel
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description,
                        Color = c.Color,
                        IsActive = c.IsActive,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt,
                        BookCount = c.BookCount
                    }).ToList();

                    return View(categories);
                }

                TempData["ErrorMessage"] = result.Message ?? "Không thể tải danh sách thể loại.";
                return View(new List<ViewModels.CategoryViewModel>());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
                return View(new List<ViewModels.CategoryViewModel>());
            }
        }

        // POST: Admin/CreateCategory - Create Category
        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] ViewModels.CreateCategoryViewModel model)
        {
            var accessCheck = await CheckAdminAccessAsync();
            if (accessCheck != null) return Json(new { success = false, message = "Unauthorized" });

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors?.Count > 0)
                    .SelectMany(x => x.Value?.Errors?.Select(e => e.ErrorMessage) ?? new List<string>());
                return Json(new { success = false, message = "Dữ liệu không hợp lệ", errors = errors });
            }

            try
            {
                var result = await _categoryApiService.CreateCategoryAsync(model.ToDto());

                if (result.Success)
                {
                    return Json(new { success = true, message = result.Message });
                }

                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        // POST: Admin/UpdateCategory - Update Category
        [HttpPost]
        public async Task<IActionResult> UpdateCategory([FromBody] ViewModels.UpdateCategoryViewModel model)
        {
            var accessCheck = await CheckAdminAccessAsync();
            if (accessCheck != null) return Json(new { success = false, message = "Unauthorized" });

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors?.Count > 0)
                    .SelectMany(x => x.Value?.Errors?.Select(e => e.ErrorMessage) ?? new List<string>());
                return Json(new { success = false, message = "Dữ liệu không hợp lệ", errors = errors });
            }

            try
            {
                var result = await _categoryApiService.UpdateCategoryAsync(model.Id, model.ToDto());

                if (result.Success)
                {
                    return Json(new { success = true, message = result.Message });
                }

                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        // POST: Admin/DeleteCategory - Delete Category
        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int categoryId)
        {
            var accessCheck = await CheckAdminAccessAsync();
            if (accessCheck != null) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var result = await _categoryApiService.DeleteCategoryAsync(categoryId);

                if (result.Success)
                {
                    return Json(new { success = true, message = result.Message });
                }

                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        // GET: Admin/GetBorrowDetail/{id} - Get Borrow Record Details
        [HttpGet]
        public async Task<IActionResult> GetBorrowDetail(int id)
        {
            var accessCheck = await CheckAdminAccessAsync();
            if (accessCheck != null) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var borrowDetail = await _borrowApiService.GetBorrowDetailAsync(id);
                if (borrowDetail != null)
                {
                    return Json(new { success = true, data = borrowDetail });
                }

                return Json(new { success = false, message = "Không thể tải chi tiết giao dịch mượn" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        // GET: Admin/Borrows - Borrow Records Management
        public async Task<IActionResult> Borrows()
        {
            var accessCheck = await CheckAdminAccessAsync();
            if (accessCheck != null) return accessCheck;

            try
            {
                var token = _authApiService.GetCurrentToken();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync($"{GetApiBaseUrl()}/api/borrow/admin/all");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var borrows = JsonSerializer.Deserialize<List<BorrowRecordViewModel>>(content, _jsonOptions);
                    return View(borrows ?? new List<BorrowRecordViewModel>());
                }

                TempData["ErrorMessage"] = "Không thể tải danh sách mượn sách.";
                return View(new List<BorrowRecordViewModel>());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
                return View(new List<BorrowRecordViewModel>());
            }
        }

        // Book Management Actions
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var accessCheck = await CheckAdminAccessAsync();
            if (accessCheck != null) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var result = await _categoryApiService.GetAllCategoriesAsync(true); // includeInactive = true để lấy tất cả

                if (result.Success && result.Categories != null)
                {
                    var categories = result.Categories.Select(c => new
                    {
                        id = c.Id,
                        name = c.Name,
                        isActive = c.IsActive
                    }).ToList();

                    return Json(new { success = true, data = categories });
                }

                return Json(new { success = false, message = result.Message ?? "Could not load categories" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Borrow Management Actions
        [HttpPost("Admin/ApproveBorrowRequest/{borrowId}")]
        public async Task<IActionResult> ApproveBorrowRequest(int borrowId)
        {
            var accessCheck = await CheckAdminAccessAsync();
            if (accessCheck != null) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var result = await _borrowApiService.ApproveBorrowRequestAsync(borrowId);
                if (result)
                {
                    return Json(new { success = true, message = "Phê duyệt yêu cầu thành công." });
                }
                // Try to get more detailed error from session
                var lastError = HttpContext.Session.GetString("LastApproveError");
                if (!string.IsNullOrEmpty(lastError))
                {
                    // Clear it after reading
                    HttpContext.Session.Remove("LastApproveError");
                    return Json(new { success = false, message = lastError });
                }

                return Json(new { success = false, message = "Phê duyệt yêu cầu thất bại." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateBorrowStatus(int id, string status, string? notes)
        {
            var accessCheck = await CheckAdminAccessAsync();
            if (accessCheck != null) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                _logger.LogInformation("Updating borrow status: ID={id}, Status={status}, Notes={notes}", id, status, notes);

                var result = await _borrowApiService.UpdateBorrowStatusAsync(id, status, notes);

                _logger.LogInformation("Update result: {result}", result);

                if (result)
                {
                    _logger.LogInformation("Borrow status updated successfully for ID: {id}", id);
                    return Json(new { success = true, message = "Cập nhật trạng thái thành công" });
                }
                else
                {
                    _logger.LogWarning("Failed to update borrow status for ID: {id}", id);
                    return Json(new { success = false, message = "Không thể cập nhật trạng thái" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating borrow status for ID: {id}", id);

                // Xử lý lỗi validation trạng thái
                if (ex.Message.Contains("không thể chuyển"))
                {
                    return Json(new { success = false, message = ex.Message });
                }

                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllowedTransitions(int id)
        {
            var accessCheck = await CheckAdminAccessAsync();
            if (accessCheck != null) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var token = _authApiService.GetCurrentToken();
                if (string.IsNullOrEmpty(token))
                {
                    return Json(new { success = false, message = "Không có token xác thực" });
                }

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                var response = await _httpClient.GetAsync($"{GetApiBaseUrl()}/api/borrow/admin/{id}/allowed-transitions");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return Json(new { success = true, data = JsonSerializer.Deserialize<object>(content, _jsonOptions) });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể lấy danh sách trạng thái hợp lệ" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        private async Task UpdateBorrowNotesInternal(int id, string? notes)
        {
            var token = _authApiService.GetCurrentToken();
            if (string.IsNullOrEmpty(token))
            {
                throw new Exception("Không có token xác thực");
            }

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            var notesDto = new { Notes = notes };
            var json = JsonSerializer.Serialize(notesDto, _jsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"{GetApiBaseUrl()}/api/borrow/admin/{id}/notes", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Không thể cập nhật ghi chú");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ExtendDueDate(int id, DateTime newDueDate, string? reason)
        {
            var accessCheck = await CheckAdminAccessAsync();
            if (accessCheck != null) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var token = _authApiService.GetCurrentToken();
                if (string.IsNullOrEmpty(token))
                {
                    return Json(new { success = false, message = "Không có token xác thực" });
                }

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                var extendDto = new { NewDueDate = newDueDate, Reason = reason };
                var json = JsonSerializer.Serialize(extendDto, _jsonOptions);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"{GetApiBaseUrl()}/api/borrow/admin/{id}/extend", content);

                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true, message = "Gia hạn thành công" });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể gia hạn" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SendReminder(int id)
        {
            var accessCheck = await CheckAdminAccessAsync();
            if (accessCheck != null) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var token = _authApiService.GetCurrentToken();
                if (string.IsNullOrEmpty(token))
                {
                    return Json(new { success = false, message = "Không có token xác thực" });
                }

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                var response = await _httpClient.PostAsync($"{GetApiBaseUrl()}/api/borrow/admin/{id}/remind", null);

                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true, message = "Đã gửi thông báo nhắc nhở" });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể gửi thông báo" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmReturn(int id)
        {
            var accessCheck = await CheckAdminAccessAsync();
            if (accessCheck != null) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var token = _authApiService.GetCurrentToken();
                if (string.IsNullOrEmpty(token))
                {
                    return Json(new { success = false, message = "Không có token xác thực" });
                }

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                var response = await _httpClient.PostAsync($"{GetApiBaseUrl()}/api/borrow/admin/{id}/return", null);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var returnResult = JsonSerializer.Deserialize<dynamic>(result, _jsonOptions);
                    return Json(new { success = true, message = "Xác nhận trả sách thành công", data = returnResult });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể xác nhận trả sách" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        [HttpPost]
        public IActionResult ExtendBorrow(int id, string? reason = null)
        {
            return Json(new { success = false, message = "Chức năng gia hạn sách đã bị vô hiệu hóa." });
        }

        // GET: Admin/GetUserDetail/{id}
        [HttpGet]
        public async Task<IActionResult> GetUserDetail(string id)
        {
            var accessCheck = await CheckAdminAccessAsync();
            if (accessCheck != null) return PartialView("_UserDetailPartial", null);

            try
            {
                var token = _authApiService.GetCurrentToken();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // Get user info
                var userResponse = await _httpClient.GetAsync($"{GetApiBaseUrl()}/api/User/admin/{id}");
                AdminUserViewModel? user = null;
                if (userResponse.IsSuccessStatusCode)
                {
                    var userContent = await userResponse.Content.ReadAsStringAsync();
                    user = JsonSerializer.Deserialize<AdminUserViewModel>(userContent, _jsonOptions);
                }

                // Get user's borrow history
                var borrowResponse = await _httpClient.GetAsync($"{GetApiBaseUrl()}/api/Borrow/user/{id}");
                List<UserBorrowedBookViewModel> borrows = new List<UserBorrowedBookViewModel>();
                if (borrowResponse.IsSuccessStatusCode)
                {
                    var borrowContent = await borrowResponse.Content.ReadAsStringAsync();
                    borrows = JsonSerializer.Deserialize<List<UserBorrowedBookViewModel>>(borrowContent, _jsonOptions) ?? new List<UserBorrowedBookViewModel>();
                }

                // Create a combined view model
                var viewModel = new UserDetailWithBorrowsViewModel
                {
                    User = user,
                    BorrowedBooks = borrows
                };

                return PartialView("_UserDetailPartial", viewModel);
            }
            catch (Exception)
            {
                return PartialView("_UserDetailPartial", null);
            }
        }

        // POST: Admin/DisableUser/{id}
        [HttpPost]
        public async Task<IActionResult> DisableUser(string id)
        {
            var accessCheck = await CheckAdminAccessAsync();
            if (accessCheck != null) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var token = _authApiService.GetCurrentToken();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsync($"{GetApiBaseUrl()}/api/User/{id}/disable", null);
                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true, message = "Đã vô hiệu hóa sinh viên thành công" });
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return Json(new { success = false, message = $"Lỗi: {response.StatusCode}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        // POST: Admin/EnableUser/{id}
        [HttpPost]
        public async Task<IActionResult> EnableUser(string id)
        {
            var accessCheck = await CheckAdminAccessAsync();
            if (accessCheck != null) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var token = _authApiService.GetCurrentToken();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsync($"{GetApiBaseUrl()}/api/User/{id}/enable", null);
                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true, message = "Đã kích hoạt sinh viên thành công" });
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return Json(new { success = false, message = $"Lỗi: {response.StatusCode}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        // GET: Admin/UserBorrows/{id} - DEPRECATED: Functionality moved to inline user details modal
        /*
        public async Task<IActionResult> UserBorrows(string id)
        {
            var accessCheck = await CheckAdminAccessAsync();
            if (accessCheck != null) return accessCheck;

            try
            {
                var token = _authApiService.GetCurrentToken();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // Get user info
                var userResponse = await _httpClient.GetAsync($"{GetApiBaseUrl()}/api/User/admin/{id}");
                AdminUserViewModel? user = null;
                if (userResponse.IsSuccessStatusCode)
                {
                    var userContent = await userResponse.Content.ReadAsStringAsync();
                    user = JsonSerializer.Deserialize<AdminUserViewModel>(userContent, _jsonOptions);
                }

                // Get user's borrow history
                var borrowResponse = await _httpClient.GetAsync($"{GetApiBaseUrl()}/api/Borrow/user/{id}");
                List<UserBorrowedBookViewModel> borrows = new List<UserBorrowedBookViewModel>();
                if (borrowResponse.IsSuccessStatusCode)
                {
                    var borrowContent = await borrowResponse.Content.ReadAsStringAsync();
                    borrows = JsonSerializer.Deserialize<List<UserBorrowedBookViewModel>>(borrowContent, _jsonOptions) ?? new List<UserBorrowedBookViewModel>();
                }

                ViewBag.User = user;
                return View(borrows);
            }
            catch (Exception)
            {
                ViewBag.User = null;
                return View(new List<UserBorrowedBookViewModel>());
            }
        }
        */

        // GET: Admin/EditUser/{id}
        public async Task<IActionResult> EditUser(string id)
        {
            var accessCheck = await CheckAdminAccessAsync();
            if (accessCheck != null) return accessCheck;

            try
            {
                var token = _authApiService.GetCurrentToken();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync($"{GetApiBaseUrl()}/api/User/admin/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var user = JsonSerializer.Deserialize<AdminUserViewModel>(content, _jsonOptions);
                    return View(user);
                }
                return NotFound();
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        // GET: Admin/DebugApi - Debug page
        public IActionResult DebugApi()
        {
            return View();
        }

        // GET: Admin/Books - Quản lý sách
        public async Task<IActionResult> Books()
        {
            var accessCheck = await CheckAdminAccessAsync();
            if (accessCheck != null) return accessCheck;

            try
            {
                var token = _authApiService.GetCurrentToken();
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // Get all books from API
                var response = await _httpClient.GetAsync($"{GetApiBaseUrl()}/api/Book/admin/all");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    List<BookDto> bookDtos = new();

                    try
                    {
                        if (!string.IsNullOrEmpty(content))
                        {
                            bookDtos = JsonSerializer.Deserialize<List<BookDto>>(content, _jsonOptions) ?? new();
                        }
                    }
                    catch (Exception ex)
                    {
                        TempData["BooksError"] = $"Books Deserialization Error: {ex.Message}";
                    }

                    // Map BookDto to AdminBookViewModel
                    var books = bookDtos?.Select(dto => new AdminBookViewModel
                    {
                        Id = dto.Id,
                        Title = dto.Title ?? string.Empty,
                        Author = dto.Author ?? string.Empty,
                        ISBN = dto.ISBN ?? string.Empty,
                        Publisher = dto.Publisher ?? string.Empty,
                        PublicationYear = dto.PublicationYear,
                        Description = dto.Description ?? string.Empty,
                        ImageUrl = dto.CoverImageUrl ?? string.Empty,
                        TotalQuantity = dto.Quantity,
                        AvailableQuantity = dto.AvailableQuantity,
                        RequestedCount = dto.RequestedCount,
                        Language = dto.Language ?? string.Empty,
                        PageCount = dto.PageCount,
                        AverageRating = (decimal)dto.AverageRating,
                        RatingCount = dto.RatingCount,
                        Categories = dto.Categories?.Select(c => new CategoryInfo
                        {
                            Id = c.Id,
                            Name = c.Name ?? string.Empty,
                            Color = c.Color ?? string.Empty
                        }).ToList() ?? new List<CategoryInfo>()
                    }).ToList() ?? new List<AdminBookViewModel>();

                    // Get categories for filtering
                    var categoriesResponse = await _httpClient.GetAsync($"{GetApiBaseUrl()}/api/Category");
                    List<ELibraryManagement.Web.Models.DTOs.CategoryDto> categories = new();

                    if (categoriesResponse.IsSuccessStatusCode)
                    {
                        var categoriesContent = await categoriesResponse.Content.ReadAsStringAsync();
                        try
                        {
                            if (!string.IsNullOrEmpty(categoriesContent))
                            {
                                var categoriesResponseDto = JsonSerializer.Deserialize<ELibraryManagement.Web.Models.DTOs.CategoriesListResponseDto>(categoriesContent, _jsonOptions);
                                if (categoriesResponseDto?.Success == true)
                                {
                                    categories = categoriesResponseDto.Categories ?? new();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            TempData["CategoriesError"] = $"Categories Deserialization Error: {ex.Message}";
                        }
                    }

                    ViewBag.Categories = categories;

                    // Calculate correct statistics
                    ViewBag.TotalBooks = books?.Sum(b => b.TotalQuantity) ?? 0;  // Tổng số cuốn sách
                    ViewBag.AvailableBooks = books?.Sum(b => b.AvailableQuantity) ?? 0;  // Tổng số cuốn sách có sẵn
                    ViewBag.BorrowedBooks = books?.Sum(b => b.TotalQuantity - b.AvailableQuantity) ?? 0;  // Tổng số cuốn sách đang được mượn
                    ViewBag.RequestedBooks = books?.Sum(b => b.RequestedCount) ?? 0;  // Tổng số cuốn sách đã được đăng ký
                    ViewBag.OutOfStockBooks = books?.Count(b => b.AvailableQuantity == 0) ?? 0;  // Số loại sách hết hàng

                    return View(books ?? new List<AdminBookViewModel>());
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể tải danh sách sách. Vui lòng thử lại.";
                    return View(new List<AdminBookViewModel>());
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
                return View(new List<AdminBookViewModel>());
            }
        }

        // GET: Admin/TestBooks - Test Books API endpoint  
        public async Task<IActionResult> TestBooks()
        {
            try
            {
                var token = _authApiService.GetCurrentToken();
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync($"{GetApiBaseUrl()}/api/Book/admin/all");
                var content = await response.Content.ReadAsStringAsync();

                return Json(new
                {
                    statusCode = response.StatusCode,
                    isSuccess = response.IsSuccessStatusCode,
                    content = content,
                    apiUrl = $"{GetApiBaseUrl()}/api/Book/admin/all",
                    hasToken = !string.IsNullOrEmpty(token)
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    error = ex.Message,
                    apiUrl = $"{GetApiBaseUrl()}/api/Book/admin/all"
                });
            }
        }

        // GET: Admin/TestApiUsers - Test API endpoint
        public async Task<IActionResult> TestApiUsers()
        {
            try
            {
                var token = _authApiService.GetCurrentToken();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync($"{GetApiBaseUrl()}/api/User");
                var content = await response.Content.ReadAsStringAsync();

                return Json(new
                {
                    statusCode = response.StatusCode,
                    isSuccess = response.IsSuccessStatusCode,
                    content = content,
                    apiUrl = $"{GetApiBaseUrl()}/api/User",
                    hasToken = !string.IsNullOrEmpty(token)
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    error = ex.Message,
                    apiUrl = $"{GetApiBaseUrl()}/api/User"
                });
            }
        }

        // GET: Admin/BookDetail/{id} - Xem chi tiết sách
        [HttpGet]
        public async Task<IActionResult> BookDetail(int id)
        {
            var accessCheck = await CheckAdminAccessAsync();
            if (accessCheck != null) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var token = _authApiService.GetCurrentToken();
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync($"{GetApiBaseUrl()}/api/Book/admin/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    if (!string.IsNullOrEmpty(content))
                    {
                        var bookDto = JsonSerializer.Deserialize<BookDto>(content, _jsonOptions);

                        if (bookDto != null)
                        {
                            var bookDetail = new AdminBookViewModel
                            {
                                Id = bookDto.Id,
                                Title = bookDto.Title ?? string.Empty,
                                Author = bookDto.Author ?? string.Empty,
                                ISBN = bookDto.ISBN ?? string.Empty,
                                Publisher = bookDto.Publisher ?? string.Empty,
                                PublicationYear = bookDto.PublicationYear,
                                Description = bookDto.Description ?? string.Empty,
                                ImageUrl = bookDto.CoverImageUrl ?? string.Empty,
                                TotalQuantity = bookDto.Quantity,
                                AvailableQuantity = bookDto.AvailableQuantity,
                                RequestedCount = bookDto.RequestedCount,
                                Language = bookDto.Language ?? string.Empty,
                                PageCount = bookDto.PageCount,
                                AverageRating = (decimal)bookDto.AverageRating,
                                RatingCount = bookDto.RatingCount,
                                Categories = bookDto.Categories?.Select(c => new CategoryInfo
                                {
                                    Id = c.Id,
                                    Name = c.Name ?? string.Empty,
                                    Color = c.Color ?? string.Empty
                                }).ToList() ?? new List<CategoryInfo>()
                            };

                            return PartialView("_BookDetailModal", bookDetail);
                        }
                    }
                }

                return PartialView("_BookDetailModal", null);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        // POST: Admin/UploadImage - Upload ảnh sách
        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            var accessCheck = await CheckAdminAccessAsync();
            if (accessCheck != null) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                if (file == null || file.Length == 0)
                {
                    return Json(new { success = false, message = "Không có file được chọn" });
                }

                // Validate file type
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
                if (!allowedTypes.Contains(file.ContentType.ToLower()))
                {
                    return Json(new { success = false, message = "Chỉ chấp nhận file ảnh (JPG, PNG, GIF, WEBP)" });
                }

                // Validate file size (max 5MB)
                if (file.Length > 5 * 1024 * 1024)
                {
                    return Json(new { success = false, message = "File ảnh không được vượt quá 5MB" });
                }

                var token = _authApiService.GetCurrentToken();
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                // Create multipart form data
                using var formData = new MultipartFormDataContent();
                using var fileContent = new StreamContent(file.OpenReadStream());
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                formData.Add(fileContent, "file", file.FileName);

                var response = await _httpClient.PostAsync($"{GetApiBaseUrl()}/api/Book/admin/upload-image", formData);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);

                    if (result.TryGetProperty("imageUrl", out var imageUrlProperty))
                    {
                        var imageUrl = imageUrlProperty.GetString();
                        return Json(new { success = true, imageUrl = imageUrl, message = "Upload ảnh thành công" });
                    }

                    return Json(new { success = true, message = "Upload thành công nhưng không nhận được URL" });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    try
                    {
                        var errorResult = JsonSerializer.Deserialize<JsonElement>(errorContent, _jsonOptions);
                        if (errorResult.TryGetProperty("message", out var messageProperty))
                        {
                            return Json(new { success = false, message = messageProperty.GetString() });
                        }
                    }
                    catch
                    {
                        // Ignore JSON parse error, use status message instead
                    }
                    return Json(new { success = false, message = $"Lỗi upload: {response.StatusCode} - {errorContent}" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi không xác định: {ex.Message}" });
            }
        }

        // POST: Admin/SaveBook - Lưu sách (tạo mới hoặc cập nhật)
        [HttpPost]
        public async Task<IActionResult> SaveBook([FromBody] JsonElement bookData)
        {
            var accessCheck = await CheckAdminAccessAsync();
            if (accessCheck != null) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                if (bookData.ValueKind == JsonValueKind.Null || bookData.ValueKind == JsonValueKind.Undefined)
                {
                    return Json(new { success = false, message = "Dữ liệu sách không hợp lệ" });
                }

                // Parse basic required fields
                if (!bookData.TryGetProperty("Title", out var titleProperty) ||
                    string.IsNullOrWhiteSpace(titleProperty.GetString()))
                {
                    return Json(new { success = false, message = "Tiêu đề sách là bắt buộc" });
                }

                if (!bookData.TryGetProperty("Author", out var authorProperty) ||
                    string.IsNullOrWhiteSpace(authorProperty.GetString()))
                {
                    return Json(new { success = false, message = "Tác giả là bắt buộc" });
                }

                if (!bookData.TryGetProperty("Quantity", out var quantityProperty) ||
                    quantityProperty.GetInt32() <= 0)
                {
                    return Json(new { success = false, message = "Số lượng phải lớn hơn 0" });
                }

                var token = _authApiService.GetCurrentToken();
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                string apiUrl;
                HttpResponseMessage response;

                // Check if it's an update (has ID > 0) or create new
                var isUpdate = bookData.TryGetProperty("Id", out var idProperty) && idProperty.GetInt32() > 0;

                if (isUpdate)
                {
                    // Update existing book
                    var bookId = idProperty.GetInt32();
                    apiUrl = $"{GetApiBaseUrl()}/api/Book/admin/update";
                    var json = bookData.GetRawText();
                    var content = new StringContent(json, System.Text.Encoding.UTF8, new System.Net.Http.Headers.MediaTypeHeaderValue("application/json"));
                    response = await _httpClient.PutAsync(apiUrl, content);
                }
                else
                {
                    // Create new book
                    apiUrl = $"{GetApiBaseUrl()}/api/Book/admin/create";
                    var json = bookData.GetRawText();
                    var content = new StringContent(json, System.Text.Encoding.UTF8, new System.Net.Http.Headers.MediaTypeHeaderValue("application/json"));
                    response = await _httpClient.PostAsync(apiUrl, content);
                }

                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true, message = isUpdate ? "Cập nhật sách thành công" : "Thêm sách mới thành công" });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Json(new { success = false, message = $"Lỗi từ API: {response.StatusCode} - {errorContent}" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        // GET: Admin/GetBookDetail/{id} - Lấy thông tin sách để chỉnh sửa
        [HttpGet]
        public async Task<IActionResult> GetBookDetail(int id)
        {
            var accessCheck = await CheckAdminAccessAsync();
            if (accessCheck != null) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var token = _authApiService.GetCurrentToken();
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                var response = await _httpClient.GetAsync($"{GetApiBaseUrl()}/api/Book/admin/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(content))
                    {
                        var bookDto = JsonSerializer.Deserialize<BookDto>(content, _jsonOptions);
                        if (bookDto != null)
                        {
                            return Json(new { success = true, data = bookDto });
                        }
                    }
                }

                return Json(new { success = false, message = "Không tìm thấy sách" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        // POST: Admin/DeleteBook - Xóa sách
        [HttpPost]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var accessCheck = await CheckAdminAccessAsync();
            if (accessCheck != null) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var token = _authApiService.GetCurrentToken();
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                var response = await _httpClient.DeleteAsync($"{GetApiBaseUrl()}/api/Book/{id}");

                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true, message = "Xóa sách thành công" });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Json(new { success = false, message = $"Lỗi từ API: {response.StatusCode} - {errorContent}" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        #region Fine Management Actions

        // GET: Admin/Fines - Quản lý phạt
        public async Task<IActionResult> Fines(int page = 1, string? status = null, string? search = null)
        {
            var accessCheck = await CheckAdminAccessAsync();
            if (accessCheck != null) return accessCheck;

            try
            {
                var token = _authApiService.GetCurrentToken();
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // Set token cho FineApiService
                _fineApiService.SetAuthToken(token);

                var (fines, totalCount, totalPages) = await _fineApiService.GetAllFinesAsync(page, 20, status, search);
                var statistics = await _fineApiService.GetFineStatisticsAsync();

                ViewBag.Statistics = statistics;
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalCount = totalCount;
                ViewBag.CurrentStatus = status;
                ViewBag.CurrentSearch = search;

                return View(fines);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
                return View(new List<FineViewModel>());
            }
        }

        // GET: Admin/FineDetails/{id} - Chi tiết phạt
        [HttpGet]
        public async Task<IActionResult> FineDetails(int id)
        {
            var accessCheck = await CheckAdminAccessAsync();
            if (accessCheck != null) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var token = _authApiService.GetCurrentToken();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // Set token cho FineApiService
                _fineApiService.SetAuthToken(token);

                var fineDetail = await _fineApiService.GetFineDetailsAsync(id);
                if (fineDetail != null)
                {
                    return PartialView("_FineDetailModal", fineDetail);
                }

                return Json(new { success = false, message = "Không tìm thấy thông tin phạt" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        // GET: Admin/CreateFine - Tạo phạt mới
        [HttpGet]
        public async Task<IActionResult> CreateFine(string? userId = null)
        {
            var accessCheck = await CheckAdminAccessAsync();
            if (accessCheck != null) return accessCheck;

            try
            {
                // Get all users for dropdown
                var token = _authApiService.GetCurrentToken();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var usersResponse = await _httpClient.GetAsync($"{GetApiBaseUrl()}/api/User");
                List<AdminUserViewModel> users = new List<AdminUserViewModel>();

                if (usersResponse.IsSuccessStatusCode)
                {
                    var usersContent = await usersResponse.Content.ReadAsStringAsync();
                    users = JsonSerializer.Deserialize<List<AdminUserViewModel>>(usersContent, _jsonOptions) ?? new List<AdminUserViewModel>();
                }

                ViewBag.Users = users.Where(u => u.Roles.Any(r => r.Equals("User", StringComparison.OrdinalIgnoreCase))).ToList();
                ViewBag.SelectedUserId = userId;

                return View(new CreateFineRequest());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
                return RedirectToAction("Fines");
            }
        }

        // POST: Admin/CreateFine - Tạo phạt mới
        [HttpPost]
        public async Task<IActionResult> CreateFine(CreateFineRequest model)
        {
            var accessCheck = await CheckAdminAccessAsync();
            if (accessCheck != null) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                // Reload users for dropdown
                var token = _authApiService.GetCurrentToken();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var usersResponse = await _httpClient.GetAsync($"{GetApiBaseUrl()}/api/User");
                List<AdminUserViewModel> users = new List<AdminUserViewModel>();

                if (usersResponse.IsSuccessStatusCode)
                {
                    var usersContent = await usersResponse.Content.ReadAsStringAsync();
                    users = JsonSerializer.Deserialize<List<AdminUserViewModel>>(usersContent, _jsonOptions) ?? new List<AdminUserViewModel>();
                }

                ViewBag.Users = users.Where(u => u.Roles.Any(r => r.Equals("User", StringComparison.OrdinalIgnoreCase))).ToList();
                return View(model);
            }

            try
            {
                var token = _authApiService.GetCurrentToken();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var success = await _fineApiService.CreateFineAsync(model);

                if (success)
                {
                    TempData["SuccessMessage"] = "Tạo phạt thành công";
                    return RedirectToAction("Fines");
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể tạo phạt. Vui lòng thử lại.";

                    // Reload users for dropdown
                    var usersResponse = await _httpClient.GetAsync($"{GetApiBaseUrl()}/api/User");
                    List<AdminUserViewModel> users = new List<AdminUserViewModel>();

                    if (usersResponse.IsSuccessStatusCode)
                    {
                        var usersContent = await usersResponse.Content.ReadAsStringAsync();
                        users = JsonSerializer.Deserialize<List<AdminUserViewModel>>(usersContent, _jsonOptions) ?? new List<AdminUserViewModel>();
                    }

                    ViewBag.Users = users.Where(u => u.Roles.Any(r => r.Equals("User", StringComparison.OrdinalIgnoreCase))).ToList();
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
                return RedirectToAction("Fines");
            }
        }

        // POST: Admin/MarkFineAsPaid - Đánh dấu phạt đã thanh toán
        [HttpPost]
        public async Task<IActionResult> MarkFineAsPaid(int id, string? notes = null)
        {
            var accessCheck = await CheckAdminAccessAsync();
            if (accessCheck != null) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var token = _authApiService.GetCurrentToken();
                if (string.IsNullOrEmpty(token))
                {
                    return Json(new { success = false, message = "Không có token xác thực" });
                }

                // Set token cho FineApiService
                _fineApiService.SetAuthToken(token);

                var success = await _fineApiService.MarkFineAsPaidAsync(id, notes);

                if (success)
                {
                    return Json(new { success = true, message = "Đã đánh dấu phạt là đã thanh toán và cập nhật trạng thái sách đã trả" });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể cập nhật trạng thái phạt" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        // POST: Admin/WaiveFine - Miễn phạt
        [HttpPost]
        public async Task<IActionResult> WaiveFine(int id, string reason, string? notes = null)
        {
            var accessCheck = await CheckAdminAccessAsync();
            if (accessCheck != null) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var token = _authApiService.GetCurrentToken();
                if (string.IsNullOrEmpty(token))
                {
                    return Json(new { success = false, message = "Không có token xác thực" });
                }

                // Set token cho FineApiService
                _fineApiService.SetAuthToken(token);

                var success = await _fineApiService.WaiveFineAsync(id, reason, notes);

                if (success)
                {
                    return Json(new { success = true, message = "Đã miễn phạt thành công" });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể miễn phạt" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        // POST: Admin/UpdateFine - Cập nhật phạt
        [HttpPost]
        public async Task<IActionResult> UpdateFine(int id, [FromBody] UpdateFineRequest model)
        {
            var accessCheck = await CheckAdminAccessAsync();
            if (accessCheck != null) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var token = _authApiService.GetCurrentToken();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var success = await _fineApiService.UpdateFineAsync(id, model);

                if (success)
                {
                    return Json(new { success = true, message = "Cập nhật phạt thành công" });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể cập nhật phạt" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        // GET: Admin/GetUserFines/{userId} - Lấy danh sách phạt của user
        [HttpGet]
        public async Task<IActionResult> GetUserFines(string userId)
        {
            var accessCheck = await CheckAdminAccessAsync();
            if (accessCheck != null) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var token = _authApiService.GetCurrentToken();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var fines = await _fineApiService.GetUserFinesAsync(userId);
                return Json(new { success = true, data = fines });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        // GET: Admin/GetUserActiveBorrowRecords/{userId} - Lấy borrow records đang mượn/quá hạn của user
        [HttpGet]
        public async Task<IActionResult> GetUserActiveBorrowRecords(string userId)
        {
            var accessCheck = await CheckAdminAccessAsync();
            if (accessCheck != null) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var token = _authApiService.GetCurrentToken();

                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("GetUserActiveBorrowRecords missing token for user {UserId}", userId);
                    return Json(new { success = false, message = "MissingAuthToken", detail = "Auth token not found in session. Please login again." });
                }

                // Create a new HttpClient instance to avoid header conflicts
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                _logger.LogInformation("Calling API: {Url} with token ending in: {TokenEnd}",
                    $"{GetApiBaseUrl()}/api/Borrow/user/{userId}/active",
                    token.Length > 10 ? token.Substring(token.Length - 10) : token);

                var response = await httpClient.GetAsync($"{GetApiBaseUrl()}/api/Borrow/user/{userId}/active");
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var borrowRecords = JsonSerializer.Deserialize<List<BorrowRecordViewModel>>(content, _jsonOptions) ?? new List<BorrowRecordViewModel>();
                    _logger.LogInformation("GetUserActiveBorrowRecords returned {Count} records for user {UserId}", borrowRecords.Count, userId);
                    return Json(new { success = true, data = borrowRecords });
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("GetUserActiveBorrowRecords API returned 401 Unauthorized for user {UserId}. Token may be expired.", userId);
                    return Json(new { success = false, message = "TokenExpired", detail = "Authentication token has expired. Please login again." });
                }
                else
                {
                    _logger.LogWarning("GetUserActiveBorrowRecords API returned {Status} for user {UserId}: {Content}", response.StatusCode, userId, content);
                    return Json(new { success = false, message = "Không thể tải dữ liệu borrow records từ API.", detail = content, status = (int)response.StatusCode });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUserActiveBorrowRecords for user {UserId}", userId);
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        #endregion

        // GET: Admin/DebugAuthToken - DEBUG ONLY: kiểm tra token trong session
        [HttpGet]
        public async Task<IActionResult> DebugAuthToken()
        {
            var accessCheck = await CheckAdminAccessAsync();
            if (accessCheck != null) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var token = _authApiService.GetCurrentToken();
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogInformation("DebugAuthToken: no token in session");
                    return Json(new { success = true, hasToken = false });
                }

                var preview = token.Length > 10 ? token.Substring(token.Length - 10) : token;
                _logger.LogInformation("DebugAuthToken: token present, preview={Preview}", preview);
                return Json(new { success = true, hasToken = true, tokenPreview = preview });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DebugAuthToken");
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}