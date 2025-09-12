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
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly JsonSerializerOptions _jsonOptions;

        public AdminController(
            IAuthApiService authApiService,
            IBookApiService bookApiService,
            IReviewApiService reviewApiService,
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _authApiService = authApiService;
            _bookApiService = bookApiService;
            _reviewApiService = reviewApiService;
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
                var books = await _bookApiService.GetAvailableBooksAsync();
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

                var response = await _httpClient.GetAsync($"{GetApiBaseUrl()}/api/Book/borrow-records");

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
    }
}