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

                // Get total books count
                try
                {
                    var booksResponse = await _httpClient.GetAsync($"{GetApiBaseUrl()}/api/Book/admin/all");
                    if (booksResponse.IsSuccessStatusCode)
                    {
                        var booksContent = await booksResponse.Content.ReadAsStringAsync();
                        var books = JsonSerializer.Deserialize<List<BookViewModel>>(booksContent, _jsonOptions);
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

                // Debug logging - temporary
                Console.WriteLine($"Loaded {books.Count} books from API");
                foreach (var book in books.Take(3))
                {
                    Console.WriteLine($"Book: {book.Title}, Total: {book.TotalCopies}, Available: {book.AvailableCopies}");
                }

                return View(books ?? new List<BookViewModel>());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading books: {ex.Message}");
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
                return View(new List<BookViewModel>());
            }
        }

        // Debug API - temporary action to check raw API response
        [HttpGet]
        public async Task<IActionResult> DebugApiBooks()
        {
            try
            {
                var token = _authApiService.GetCurrentToken();
                var apiBaseUrl = GetApiBaseUrl();
                var url = $"{apiBaseUrl}/api/book/admin/all";

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                ViewBag.ApiUrl = url;
                ViewBag.ResponseStatus = response.StatusCode;
                ViewBag.RawResponse = content;

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        // Debug: Test create book with sample data
        [HttpGet]
        public async Task<IActionResult> TestCreateBook()
        {
            try
            {
                var sampleBook = new CreateBookViewModel
                {
                    Title = "Test Book " + DateTime.Now.ToString("HHmmss"),
                    Author = "Test Author",
                    ISBN = "9999999999999",
                    Publisher = "Test Publisher",
                    PublicationYear = 2024,
                    Description = "This is a test book created for debugging",
                    CoverImageUrl = "https://via.placeholder.com/300x400",
                    Quantity = 5,
                    Language = "Tiếng Việt",
                    PageCount = 200,
                    CategoryIds = new List<int> { 1 } // Assuming category ID 1 exists
                };

                var token = _authApiService.GetCurrentToken();
                if (string.IsNullOrEmpty(token))
                {
                    return Json(new { success = false, message = "No authentication token" });
                }

                var result = await _bookApiService.CreateBookAsync(sampleBook, token);
                return Json(new { success = true, message = "Test book created successfully", data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, stackTrace = ex.StackTrace });
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

        [HttpGet]
        public async Task<IActionResult> GetBookDetail(int id)
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

                var book = await _bookApiService.GetBookDetailAsync(id, token);
                if (book == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy sách" });
                }

                return Json(new { success = true, data = book });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateBook([FromBody] CreateBookViewModel model)
        {
            var accessCheck = await CheckAdminAccessAsync();
            if (accessCheck != null) return Json(new { success = false, message = "Unauthorized" });

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors?.Count > 0)
                    .Select(x => new
                    {
                        Field = x.Key,
                        Errors = x.Value?.Errors?.Select(e => e.ErrorMessage) ?? new List<string>()
                    });

                var errorMessage = string.Join("; ", errors.SelectMany(e => e.Errors));

                return Json(new
                {
                    success = false,
                    message = $"Dữ liệu không hợp lệ: {errorMessage}",
                    errors = errors
                });
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

        [HttpPost]
        public async Task<IActionResult> UploadBookImage(IFormFile file)
        {
            try
            {
                if (!await IsAdminAsync())
                {
                    return Json(new { success = false, message = "Bạn không có quyền thực hiện chức năng này!" });
                }

                if (file == null || file.Length == 0)
                {
                    return Json(new { success = false, message = "Vui lòng chọn file ảnh!" });
                }

                var token = _authApiService.GetCurrentToken();
                if (string.IsNullOrEmpty(token))
                {
                    return Json(new { success = false, message = "Bạn cần đăng nhập để thực hiện chức năng này!" });
                }

                var result = await _bookApiService.UploadBookImageAsync(file, token);

                return Json(new
                {
                    success = result.Success,
                    message = result.Message,
                    imageUrl = result.ImageUrl
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }
    }
}