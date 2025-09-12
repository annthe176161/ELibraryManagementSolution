using ELibraryManagement.Web.Models;
using ELibraryManagement.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace ELibraryManagement.Web.Controllers
{
    public class BookController : Controller
    {
        private readonly IBookApiService _bookApiService;
        private readonly IAuthApiService _authApiService;

        public BookController(IBookApiService bookApiService, IAuthApiService authApiService)
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
                    return RedirectToAction("Login", "Account");
                }

                var token = _authApiService.GetCurrentToken();
                if (string.IsNullOrEmpty(token))
                {
                    TempData["ErrorMessage"] = "Phiên đăng nhập đã hết hạn.";
                    return RedirectToAction("Login", "Account");
                }

                // Lấy danh sách sách đã mượn
                var borrowedBooks = await _bookApiService.GetBorrowedBooksAsync(currentUser.Id, token);
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

                // Tạo ViewModel cho borrowed book details
                var viewModel = new BorrowedBookDetailViewModel
                {
                    BorrowRecord = borrowedBook,
                    BookDetails = bookDetails
                };

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
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var book = await _bookApiService.GetBookByIdAsync(id);
                if (book == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy sách.";
                    return RedirectToAction("Index", "Home");
                }

                var borrowViewModel = new BorrowBookViewModel
                {
                    BookId = book.Id,
                    BookTitle = book.Title,
                    BookAuthor = book.Author,
                    BookCoverUrl = book.ImageUrl,
                    DueDate = DateTime.Today.AddDays(14) // Mặc định 14 ngày
                };

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
                return RedirectToAction("Login", "Account");
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
                    return RedirectToAction("Login", "Account");
                }

                System.Diagnostics.Debug.WriteLine($"Current user: {currentUser.Id} - {currentUser.UserName}");

                var token = _authApiService.GetCurrentToken();
                if (string.IsNullOrEmpty(token))
                {
                    System.Diagnostics.Debug.WriteLine("Token is null or empty");
                    TempData["ErrorMessage"] = "Phiên đăng nhập đã hết hạn.";
                    return RedirectToAction("Login", "Account");
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
                    ModelState.AddModelError("", result.Message);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
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
                    return RedirectToAction("Login", "Account");
                }

                var currentUser = await _authApiService.GetCurrentUserAsync();
                if (currentUser == null)
                {
                    TempData["ErrorMessage"] = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.";
                    _authApiService.Logout();
                    return RedirectToAction("Login", "Account");
                }

                var token = _authApiService.GetCurrentToken();
                if (string.IsNullOrEmpty(token))
                {
                    TempData["ErrorMessage"] = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.";
                    _authApiService.Logout();
                    return RedirectToAction("Login", "Account");
                }

                var borrowedBooks = await _bookApiService.GetBorrowedBooksAsync(currentUser.Id, token);

                return View(borrowedBooks);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in MyBooks: {ex.Message}");
                TempData["ErrorMessage"] = $"Có lỗi xảy ra khi tải danh sách sách đã mượn: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: Book/Return/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Return(int id)
        {
            if (!_authApiService.IsAuthenticated())
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để trả sách.";
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var token = _authApiService.GetCurrentToken();
                if (string.IsNullOrEmpty(token))
                {
                    TempData["ErrorMessage"] = "Phiên đăng nhập đã hết hạn.";
                    return RedirectToAction("Login", "Account");
                }

                var result = await _bookApiService.ReturnBookAsync(id, token);

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
