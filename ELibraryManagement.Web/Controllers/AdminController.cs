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
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly JsonSerializerOptions _jsonOptions;

        public AdminController(
            IAuthApiService authApiService,
            IReviewApiService reviewApiService,
            IBorrowApiService borrowApiService,
            ICategoryApiService categoryApiService,
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _authApiService = authApiService;
            _reviewApiService = reviewApiService;
            _borrowApiService = borrowApiService;
            _categoryApiService = categoryApiService;
            _httpClient = httpClient;
            _configuration = configuration;
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

                // Get total users count
                try
                {
                    var usersResponse = await _httpClient.GetAsync($"{GetApiBaseUrl()}/api/User");
                    if (usersResponse.IsSuccessStatusCode)
                    {
                        var usersContent = await usersResponse.Content.ReadAsStringAsync();
                        var users = JsonSerializer.Deserialize<List<AdminUserViewModel>>(usersContent, _jsonOptions);
                        dashboardData.TotalUsers = users?.Count ?? 0;
                    }
                    else
                    {
                        dashboardData.TotalUsers = 0;
                    }
                }
                catch (Exception)
                {
                    dashboardData.TotalUsers = 0;
                }

                // Set TotalBooks to 0 since we removed book management
                dashboardData.TotalBooks = 0;

                // Get total borrow records count
                try
                {
                    var borrowsResponse = await _httpClient.GetAsync($"{GetApiBaseUrl()}/api/Borrow/admin/all");
                    if (borrowsResponse.IsSuccessStatusCode)
                    {
                        var borrowsContent = await borrowsResponse.Content.ReadAsStringAsync();
                        var borrows = JsonSerializer.Deserialize<List<object>>(borrowsContent, _jsonOptions);
                        dashboardData.TotalBorrows = borrows?.Count ?? 0;
                    }
                    else
                    {
                        dashboardData.TotalBorrows = 0;
                    }
                }
                catch (Exception)
                {
                    dashboardData.TotalBorrows = 0;
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
        [HttpPost]
        public async Task<IActionResult> UpdateBorrowStatus(int id, string status, string? notes)
        {
            var accessCheck = await CheckAdminAccessAsync();
            if (accessCheck != null) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var result = await _borrowApiService.UpdateBorrowStatusAsync(id, status);

                // If notes are provided, update them too
                if (!string.IsNullOrEmpty(notes))
                {
                    await UpdateBorrowNotesInternal(id, notes);
                }

                if (result)
                {
                    return Json(new { success = true, message = "Cập nhật trạng thái thành công" });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể cập nhật trạng thái" });
                }
            }
            catch (Exception ex)
            {
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
        public async Task<IActionResult> ExtendBorrow(int id, string? reason = null)
        {
            var accessCheck = await CheckAdminAccessAsync();
            if (accessCheck != null) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var result = await _borrowApiService.ExtendBorrowAsync(id, reason);

                return Json(new
                {
                    success = result.Success,
                    message = result.Message,
                    borrowRecordId = result.BorrowRecordId,
                    bookTitle = result.BookTitle,
                    oldDueDate = result.OldDueDate.ToString("dd/MM/yyyy"),
                    newDueDate = result.NewDueDate.ToString("dd/MM/yyyy"),
                    extensionCount = result.ExtensionCount,
                    remainingExtensions = result.RemainingExtensions
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
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

                var response = await _httpClient.GetAsync($"{GetApiBaseUrl()}/api/User/admin/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var user = JsonSerializer.Deserialize<AdminUserViewModel>(content, _jsonOptions);
                    return PartialView("_UserDetailPartial", user);
                }
                return PartialView("_UserDetailPartial", null);
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

        // GET: Admin/UserBorrows/{id}
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
                List<BorrowBookViewModel> borrows = new List<BorrowBookViewModel>();
                if (borrowResponse.IsSuccessStatusCode)
                {
                    var borrowContent = await borrowResponse.Content.ReadAsStringAsync();
                    borrows = JsonSerializer.Deserialize<List<BorrowBookViewModel>>(borrowContent, _jsonOptions) ?? new List<BorrowBookViewModel>();
                }

                ViewBag.User = user;
                return View(borrows);
            }
            catch (Exception)
            {
                ViewBag.User = null;
                return View(new List<BorrowBookViewModel>());
            }
        }

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
                    ViewBag.TotalBooks = books?.Count ?? 0;
                    ViewBag.AvailableBooks = books?.Count(b => b.AvailableQuantity > 0) ?? 0;
                    ViewBag.OutOfStockBooks = books?.Count(b => b.AvailableQuantity == 0) ?? 0;

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
    }
}