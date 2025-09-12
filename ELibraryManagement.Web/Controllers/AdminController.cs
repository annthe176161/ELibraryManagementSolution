using ELibraryManagement.Web.Models;
using ELibraryManagement.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ELibraryManagement.Web.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAuthApiService _authApiService;
        private readonly IBookApiService _bookApiService;
        private readonly IReviewApiService _reviewApiService;
        private readonly IBorrowApiService _borrowApiService;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly JsonSerializerOptions _jsonOptions;

        public AdminController(
            IAuthApiService authApiService,
            IBookApiService bookApiService,
            IReviewApiService reviewApiService,
            IBorrowApiService borrowApiService,
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _authApiService = authApiService;
            _bookApiService = bookApiService;
            _reviewApiService = reviewApiService;
            _borrowApiService = borrowApiService;
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
                var usersResponse = await _httpClient.GetAsync($"{GetApiBaseUrl()}/api/User");
                if (usersResponse.IsSuccessStatusCode)
                {
                    var usersContent = await usersResponse.Content.ReadAsStringAsync();
                    var users = JsonSerializer.Deserialize<List<AdminUserViewModel>>(usersContent, _jsonOptions);
                    dashboardData.TotalUsers = users?.Count ?? 0;
                }

                // Get total books count
                var booksResponse = await _httpClient.GetAsync($"{GetApiBaseUrl()}/api/Book");
                if (booksResponse.IsSuccessStatusCode)
                {
                    var booksContent = await booksResponse.Content.ReadAsStringAsync();
                    var books = JsonSerializer.Deserialize<List<BookViewModel>>(booksContent, _jsonOptions);
                    dashboardData.TotalBooks = books?.Count ?? 0;
                }

                // Get total borrow records count
                var borrowsResponse = await _httpClient.GetAsync($"{GetApiBaseUrl()}/api/Book/borrow-records");
                if (borrowsResponse.IsSuccessStatusCode)
                {
                    var borrowsContent = await borrowsResponse.Content.ReadAsStringAsync();
                    var borrows = JsonSerializer.Deserialize<List<object>>(borrowsContent, _jsonOptions);
                    dashboardData.TotalBorrows = borrows?.Count ?? 0;
                }

                // Get total reviews count
                var reviewsResponse = await _httpClient.GetAsync($"{GetApiBaseUrl()}/api/Review/all");
                if (reviewsResponse.IsSuccessStatusCode)
                {
                    var reviewsContent = await reviewsResponse.Content.ReadAsStringAsync();
                    var reviews = JsonSerializer.Deserialize<List<object>>(reviewsContent, _jsonOptions);
                    dashboardData.TotalReviews = reviews?.Count ?? 0;
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

        // GET: Admin/Books - Book Management
        public async Task<IActionResult> Books()
        {
            var accessCheck = await CheckAdminAccessAsync();
            if (accessCheck != null) return accessCheck;

            try
            {
                var token = _authApiService.GetCurrentToken();
                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToAction("Login", "Account");
                }

                var books = await _bookApiService.GetAllBooksAsync(token);
                return View(books ?? new List<BookViewModel>());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
                return View(new List<BookViewModel>());
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
                var token = _authApiService.GetCurrentToken();
                if (string.IsNullOrEmpty(token))
                {
                    return Json(new { success = false, message = "No token" });
                }

                var categories = await _bookApiService.GetAllCategoriesAsync(token);
                return Json(new { success = true, data = categories });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateBook([FromBody] CreateBookViewModel model)
        {
            var accessCheck = await CheckAdminAccessAsync();
            if (accessCheck != null) return Json(new { success = false, message = "Unauthorized" });

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ", errors = ModelState });
            }

            try
            {
                var token = _authApiService.GetCurrentToken();
                if (string.IsNullOrEmpty(token))
                {
                    return Json(new { success = false, message = "Không có token xác thực" });
                }

                var book = await _bookApiService.CreateBookAsync(model, token);
                return Json(new { success = true, message = "Thêm sách thành công", data = book });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateBook([FromBody] UpdateBookViewModel model)
        {
            var accessCheck = await CheckAdminAccessAsync();
            if (accessCheck != null) return Json(new { success = false, message = "Unauthorized" });

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ", errors = ModelState });
            }

            try
            {
                var token = _authApiService.GetCurrentToken();
                if (string.IsNullOrEmpty(token))
                {
                    return Json(new { success = false, message = "Không có token xác thực" });
                }

                var book = await _bookApiService.UpdateBookAsync(model, token);
                return Json(new { success = true, message = "Cập nhật sách thành công", data = book });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteBook(int id)
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

                var result = await _bookApiService.DeleteBookAsync(id, token);
                if (result)
                {
                    return Json(new { success = true, message = "Xóa sách thành công" });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể xóa sách" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
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
                var token = _authApiService.GetCurrentToken();
                if (string.IsNullOrEmpty(token))
                {
                    return Json(new { success = false, message = "Không có token xác thực" });
                }

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                var updateDto = new { Status = status, Notes = notes };
                var json = JsonSerializer.Serialize(updateDto, _jsonOptions);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"{GetApiBaseUrl()}/api/borrow/admin/{id}/status", content);

                if (response.IsSuccessStatusCode)
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
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
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
    }
}