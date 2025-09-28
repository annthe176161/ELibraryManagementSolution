using ELibraryManagement.Web.Models;
using ELibraryManagement.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ELibraryManagement.Web.Controllers
{
    public class BooksController : Controller
    {
        private readonly IBookApiService _bookApiService;
        private readonly IAuthApiService _authApiService;

        public BooksController(IBookApiService bookApiService, IAuthApiService authApiService)
        {
            _bookApiService = bookApiService;
            _authApiService = authApiService;
        }

        // GET: Book/Details/5 - Redirect to Home/BookDetail for consistency
        public IActionResult Details(int id)
        {
            // Redirect to the main BookDetail page in HomeController
            return RedirectToAction("BookDetail", "Home", new { id = id });
        }

        // GET: Book/BorrowedDetails/5 - Chi tiết sách đã mượn
        public async Task<IActionResult> BorrowedDetails(int borrowRecordId)
        {
            try
            {
                var currentUser = await _authApiService.GetCurrentUserAsync();
                if (currentUser == null)
                {
                    TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem chi tiết.";
                    return RedirectToAction("Login", "Accounts");
                }

                var token = _authApiService.GetCurrentToken();
                if (string.IsNullOrEmpty(token))
                {
                    TempData["ErrorMessage"] = "Phiên đăng nhập đã hết hạn.";
                    return RedirectToAction("Login", "Accounts");
                }

                // Lấy danh sách lịch sử mượn sách (bao gồm cả sách đã trả)
                var borrowedBooks = await _bookApiService.GetBorrowHistoryAsync(currentUser.Id, token);
                var borrowedBook = borrowedBooks.FirstOrDefault(b => b.BorrowRecordId == borrowRecordId);

                if (borrowedBook == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin mượn sách.";
                    return RedirectToAction("MyBooks");
                }

                // Lấy thông tin chi tiết sách
                var bookDetails = await _bookApiService.GetBookByIdAsync(borrowedBook.BookId);
                if (bookDetails == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin sách.";
                    return RedirectToAction("MyBooks");
                }

                // Tính toán thống kê cá nhân từ borrowedBooks (đã là lịch sử đầy đủ)
                // Treat Overdue as active so overdue items appear in "Sách đang mượn"
                var currentlyActiveBorrows = borrowedBooks?.Where(b => b.Status == "Borrowed" || b.Status == "Overdue").Count() ?? 0;
                var totalBorrowed = borrowedBooks?.Where(b => b.Status == "Borrowed" || b.Status == "Returned" || b.Status == "Overdue").Count() ?? 0; // Count Borrowed, Returned and Overdue

                // Tạo ViewModel cho borrowed book details
                var viewModel = new BorrowedBookDetailViewModel
                {
                    BorrowRecord = borrowedBook,
                    BookDetails = bookDetails
                };

                // Truyền thông tin thống kê qua ViewBag
                ViewBag.CurrentActiveBorrows = currentlyActiveBorrows;
                ViewBag.TotalBorrowed = totalBorrowed;
                // Number of times the user had overdue borrows
                ViewBag.OverdueCount = borrowedBooks?.Count(b => b.IsOverdue) ?? 0;

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
                return RedirectToAction("MyBooks");
            }
        }

        // GET: Book/Borrow/5
        public async Task<IActionResult> Borrow(int id)
        {
            // Debug: Check authentication status
            var isAuth = _authApiService.IsAuthenticated();
            var token = _authApiService.GetCurrentToken();
            var userName = _authApiService.GetCurrentUserName();

            System.Diagnostics.Debug.WriteLine($"IsAuthenticated: {isAuth}");
            System.Diagnostics.Debug.WriteLine($"Token: {token}");
            System.Diagnostics.Debug.WriteLine($"UserName: {userName}");

            if (!isAuth)
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để mượn sách.";
                return RedirectToAction("Login", "Accounts");
            }

            try
            {
                var book = await _bookApiService.GetBookByIdAsync(id);
                if (book == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy sách.";
                    return RedirectToAction("Index", "Home");
                }

                // Get current user information
                var currentUser = await _authApiService.GetCurrentUserAsync();

                // Get borrowed books count for validation
                var allBorrowedBooks = await _bookApiService.GetBorrowedBooksAsync(currentUser?.Id ?? "", token ?? "");

                // Filter only currently borrowed books (exclude requested, cancelled and returned)
                // Treat Overdue as active so overdue items still count toward the user's limit
                var currentlyBorrowedBooks = allBorrowedBooks?.Where(b =>
                    b.Status == "Borrowed" || b.Status == "Overdue").ToList() ?? new List<UserBorrowedBookViewModel>();
                var currentBorrowedCount = currentlyBorrowedBooks.Count;
                var maxBooksAllowed = 5;
                var canBorrow = currentBorrowedCount < maxBooksAllowed;

                var borrowViewModel = new BorrowBookViewModel
                {
                    BookId = book.Id,
                    BookTitle = book.Title,
                    BookAuthor = book.Author,
                    BookCoverUrl = book.ImageUrl,
                    DueDate = DateTime.Today.AddDays(14), // Default 14 days
                    StudentInfo = new StudentInfoViewModel
                    {
                        StudentId = currentUser?.StudentId ?? "SV001234567",
                        FullName = $"{currentUser?.FirstName} {currentUser?.LastName}".Trim() ?? userName ?? "Nguyễn Văn An",
                        Email = currentUser?.Email ?? "anNV@fpt.edu.vn",
                        PhoneNumber = currentUser?.PhoneNumber ?? "0123 456 789",
                        Major = "Công nghệ thông tin", // This might come from a different field or table
                        AcademicYear = "2021 - 2025", // This might be calculated from registration date
                        StudentStatus = "Đang học" // This might come from user status
                    }
                };

                // Add borrow limit information to ViewBag
                ViewBag.CurrentBorrowedCount = currentBorrowedCount;
                ViewBag.MaxBooksAllowed = maxBooksAllowed;
                ViewBag.CanBorrow = canBorrow;
                ViewBag.BorrowedBooks = currentlyBorrowedBooks;

                return View(borrowViewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: Book/Borrow
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Borrow(BorrowBookViewModel model)
        {
            // Debug logging
            System.Diagnostics.Debug.WriteLine($"=== BORROW POST START ===");
            System.Diagnostics.Debug.WriteLine($"BookId: {model.BookId}");
            System.Diagnostics.Debug.WriteLine($"Notes: {model.Notes}");
            System.Diagnostics.Debug.WriteLine($"DueDate: {model.DueDate}");

            if (!_authApiService.IsAuthenticated())
            {
                System.Diagnostics.Debug.WriteLine("User not authenticated - redirecting to login");
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để mượn sách.";
                return RedirectToAction("Login", "Accounts");
            }

            if (!ModelState.IsValid)
            {
                System.Diagnostics.Debug.WriteLine("ModelState invalid");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    System.Diagnostics.Debug.WriteLine($"Model Error: {error.ErrorMessage}");
                }
                return View(model);
            }

            try
            {
                System.Diagnostics.Debug.WriteLine("Getting current user...");
                var currentUser = await _authApiService.GetCurrentUserAsync();
                if (currentUser == null)
                {
                    System.Diagnostics.Debug.WriteLine("Current user is null");
                    TempData["ErrorMessage"] = "Không thể xác thực người dùng.";
                    return RedirectToAction("Login", "Accounts");
                }

                System.Diagnostics.Debug.WriteLine($"Current user: {currentUser.Id} - {currentUser.UserName}");

                var token = _authApiService.GetCurrentToken();
                if (string.IsNullOrEmpty(token))
                {
                    System.Diagnostics.Debug.WriteLine("Token is null or empty");
                    TempData["ErrorMessage"] = "Phiên đăng nhập đã hết hạn.";
                    return RedirectToAction("Login", "Accounts");
                }

                System.Diagnostics.Debug.WriteLine($"Token exists: {token.Substring(0, Math.Min(20, token.Length))}...");

                var request = new BorrowBookRequestViewModel
                {
                    BookId = model.BookId,
                    UserId = currentUser.Id,
                    DueDate = model.DueDate,
                    Notes = model.Notes
                };

                System.Diagnostics.Debug.WriteLine($"Calling BorrowBookAsync with BookId: {request.BookId}, UserId: {request.UserId}");
                var result = await _bookApiService.BorrowBookAsync(request, token);

                System.Diagnostics.Debug.WriteLine($"BorrowBookAsync result - Success: {result.Success}, Message: {result.Message}");

                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction("MyBooks");
                }
                else
                {
                    // When there's an error, we need to reload all ViewBag data
                    // Get borrowed books count for validation
                    var allBorrowedBooks = await _bookApiService.GetBorrowedBooksAsync(currentUser.Id, token);
                    var currentlyBorrowedBooks = allBorrowedBooks?.Where(b =>
                        b.Status == "Borrowed").ToList() ?? new List<UserBorrowedBookViewModel>();
                    var currentBorrowedCount = currentlyBorrowedBooks.Count;
                    var maxBooksAllowed = 5;
                    var canBorrow = currentBorrowedCount < maxBooksAllowed;

                    // Reload student info
                    model.StudentInfo = new StudentInfoViewModel
                    {
                        StudentId = currentUser?.StudentId ?? "SV001234567",
                        FullName = $"{currentUser?.FirstName} {currentUser?.LastName}".Trim() ?? _authApiService.GetCurrentUserName() ?? "Nguyễn Văn An",
                        Email = currentUser?.Email ?? "anNV@fpt.edu.vn",
                        PhoneNumber = currentUser?.PhoneNumber ?? "0123 456 789",
                        Major = "Công nghệ thông tin",
                        AcademicYear = "2021 - 2025",
                        StudentStatus = "Đang học"
                    };

                    // Reload ViewBag data
                    ViewBag.CurrentBorrowedCount = currentBorrowedCount;
                    ViewBag.MaxBooksAllowed = maxBooksAllowed;
                    ViewBag.CanBorrow = canBorrow;
                    ViewBag.BorrowedBooks = currentlyBorrowedBooks;

                    ModelState.AddModelError("", result.Message);
                    ViewBag.ErrorMessage = result.Message; // Add to ViewBag for easy access
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                // When there's an exception, we need to reload all ViewBag data
                try
                {
                    var currentUser = await _authApiService.GetCurrentUserAsync();
                    var token = _authApiService.GetCurrentToken();

                    if (currentUser != null && !string.IsNullOrEmpty(token))
                    {
                        var allBorrowedBooks = await _bookApiService.GetBorrowedBooksAsync(currentUser.Id, token);
                        var currentlyBorrowedBooks = allBorrowedBooks?.Where(b =>
                            b.Status == "Borrowed").ToList() ?? new List<UserBorrowedBookViewModel>();
                        var currentBorrowedCount = currentlyBorrowedBooks.Count;
                        var maxBooksAllowed = 5;
                        var canBorrow = currentBorrowedCount < maxBooksAllowed;

                        // Reload student info
                        model.StudentInfo = new StudentInfoViewModel
                        {
                            StudentId = currentUser?.StudentId ?? "SV001234567",
                            FullName = $"{currentUser?.FirstName} {currentUser?.LastName}".Trim() ?? _authApiService.GetCurrentUserName() ?? "Nguyễn Văn An",
                            Email = currentUser?.Email ?? "anNV@fpt.edu.vn",
                            PhoneNumber = currentUser?.PhoneNumber ?? "0123 456 789",
                            Major = "Công nghệ thông tin",
                            AcademicYear = "2021 - 2025",
                            StudentStatus = "Đang học"
                        };

                        // Reload ViewBag data
                        ViewBag.CurrentBorrowedCount = currentBorrowedCount;
                        ViewBag.MaxBooksAllowed = maxBooksAllowed;
                        ViewBag.CanBorrow = canBorrow;
                        ViewBag.BorrowedBooks = currentlyBorrowedBooks;
                    }
                }
                catch
                {
                    // If we can't reload the data, just continue with the error
                }

                ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
                ViewBag.ErrorMessage = $"Có lỗi xảy ra: {ex.Message}"; // Add to ViewBag for easy access
                return View(model);
            }
        }

        // GET: Book/MyBooks
        public async Task<IActionResult> MyBooks()
        {
            try
            {
                if (!_authApiService.IsAuthenticated())
                {
                    TempData["ErrorMessage"] = "Bạn cần đăng nhập để xem sách đã mượn.";
                    return RedirectToAction("Login", "Accounts");
                }

                var currentUser = await _authApiService.GetCurrentUserAsync();
                if (currentUser == null)
                {
                    TempData["ErrorMessage"] = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.";
                    _authApiService.Logout();
                    return RedirectToAction("Login", "Accounts");
                }

                var token = _authApiService.GetCurrentToken();
                if (string.IsNullOrEmpty(token))
                {
                    TempData["ErrorMessage"] = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.";
                    _authApiService.Logout();
                    return RedirectToAction("Login", "Accounts");
                }

                // Clear irrelevant success messages (like Google login success) on this page
                TempData.Remove("SuccessMessage");

                // Use GetBorrowHistoryAsync to get all borrow history including returned books
                var borrowedBooks = await _bookApiService.GetBorrowHistoryAsync(currentUser.Id, token);

                return View(borrowedBooks);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in MyBooks: {ex.Message}");
                TempData["ErrorMessage"] = $"Có lỗi xảy ra khi tải danh sách sách đã mượn: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: Book/Cancel/5 - Hủy yêu cầu mượn sách
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            if (!_authApiService.IsAuthenticated())
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để hủy yêu cầu mượn sách.";
                return RedirectToAction("Login", "Accounts");
            }

            try
            {
                var token = _authApiService.GetCurrentToken();
                if (string.IsNullOrEmpty(token))
                {
                    TempData["ErrorMessage"] = "Phiên đăng nhập đã hết hạn.";
                    return RedirectToAction("Login", "Accounts");
                }

                var result = await _bookApiService.CancelBorrowRequestAsync(id, token);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message;
                }

                return RedirectToAction("MyBooks");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
                return RedirectToAction("MyBooks");
            }
        }
    }
}
