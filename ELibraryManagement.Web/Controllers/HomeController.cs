using ELibraryManagement.Web.Models;
using ELibraryManagement.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ELibraryManagement.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBookApiService _bookApiService;
        private readonly IAuthApiService _authApiService;
        private readonly IReviewApiService _reviewApiService;

        public HomeController(
            ILogger<HomeController> logger,
            IBookApiService bookApiService,
            IAuthApiService authApiService,
            IReviewApiService reviewApiService)
        {
            _logger = logger;
            _bookApiService = bookApiService;
            _authApiService = authApiService;
            _reviewApiService = reviewApiService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Books(string? search, string? category, string? author, string? sortBy, int page = 1, int pageSize = 6)
        {
            try
            {
                // Try the new paged method first, fallback to old method if needed
                try
                {
                    var pagedBooks = await _bookApiService.GetAvailableBooksPagedAsync(search, category, author, sortBy, page, pageSize);

                    // Lấy danh sách categories và authors để hiển thị trong filter
                    var categories = await _bookApiService.GetCategoriesAsync();
                    var authors = await _bookApiService.GetAuthorsAsync();

                    ViewBag.Categories = categories;
                    ViewBag.Authors = authors;
                    ViewBag.Search = search;
                    ViewBag.Category = category;
                    ViewBag.Author = author;
                    ViewBag.SortBy = sortBy;
                    ViewBag.Page = page;
                    ViewBag.PageSize = pageSize;
                    ViewBag.TotalCount = pagedBooks.TotalCount;
                    ViewBag.TotalPages = pagedBooks.TotalPages;
                    ViewBag.HasPreviousPage = pagedBooks.HasPreviousPage;
                    ViewBag.HasNextPage = pagedBooks.HasNextPage;
                    ViewBag.StartItem = pagedBooks.StartItem;
                    ViewBag.EndItem = pagedBooks.EndItem;

                    return View(pagedBooks.Items);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Paged method failed, falling back to regular method");

                    // Fallback to old method
                    var books = await _bookApiService.GetAvailableBooksAsync(search, category, author, sortBy, page, pageSize);

                    // Lấy danh sách categories và authors để hiển thị trong filter
                    var categories = await _bookApiService.GetCategoriesAsync();
                    var authors = await _bookApiService.GetAuthorsAsync();

                    ViewBag.Categories = categories;
                    ViewBag.Authors = authors;
                    ViewBag.Search = search;
                    ViewBag.Category = category;
                    ViewBag.Author = author;
                    ViewBag.SortBy = sortBy;
                    ViewBag.Page = page;
                    ViewBag.PageSize = pageSize;

                    // Set default pagination values for fallback
                    ViewBag.TotalCount = books.Count;
                    ViewBag.TotalPages = 1;
                    ViewBag.HasPreviousPage = false;
                    ViewBag.HasNextPage = false;
                    ViewBag.StartItem = books.Any() ? 1 : 0;
                    ViewBag.EndItem = books.Count;

                    return View(books);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading books");

                // Set empty pagination values for error case
                ViewBag.Categories = new List<string>();
                ViewBag.Authors = new List<string>();
                ViewBag.Search = search;
                ViewBag.Category = category;
                ViewBag.Author = author;
                ViewBag.SortBy = sortBy;
                ViewBag.Page = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalCount = 0;
                ViewBag.TotalPages = 0;
                ViewBag.HasPreviousPage = false;
                ViewBag.HasNextPage = false;
                ViewBag.StartItem = 0;
                ViewBag.EndItem = 0;

                return View(new List<BookViewModel>());
            }
        }

        public async Task<IActionResult> BookDetail(int id)
        {
            _logger.LogInformation("BookDetail action called with ID: {BookId}", id);

            try
            {
                _logger.LogInformation("Calling API to get book with ID: {BookId}", id);
                var book = await _bookApiService.GetBookByIdAsync(id);

                if (book == null)
                {
                    _logger.LogWarning("Book with ID {BookId} not found in API response", id);
                    return NotFound($"Book with ID {id} not found");
                }

                _logger.LogInformation("Successfully retrieved book: {BookTitle}", book.Title);

                // Lấy sách liên quan (cùng thể loại)
                var relatedBooks = await _bookApiService.GetRelatedBooksAsync(id, book.CategoryName);
                ViewBag.RelatedBooks = relatedBooks ?? new List<BookViewModel>();

                // Lấy recent reviews (3 đánh giá gần nhất)
                try
                {
                    var recentReviews = await _reviewApiService.GetBookReviewsAsync(id, 1, 3);
                    ViewBag.RecentReviews = recentReviews.Reviews.Select(r => new
                    {
                        UserName = r.UserName,
                        Rating = r.Rating,
                        Comment = r.Comment,
                        CreatedAt = r.CreatedAt.ToString("dd/MM/yyyy")
                    }).ToList();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not load recent reviews for book {BookId}", id);
                    ViewBag.RecentReviews = new List<object>();
                }

                // Lấy review summary
                try
                {
                    var reviewSummary = await _reviewApiService.GetBookReviewSummaryAsync(id);
                    ViewBag.ReviewSummary = reviewSummary;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not load review summary for book {BookId}", id);
                    ViewBag.ReviewSummary = null;
                }

                // Truyền thông tin authentication vào View - sử dụng cùng logic với BorrowController
                var sessionToken = HttpContext.Session.GetString("AuthToken");
                var cookieToken = HttpContext.Request.Cookies["AuthToken"];
                var isAuthenticated = !string.IsNullOrEmpty(sessionToken) || !string.IsNullOrEmpty(cookieToken);

                ViewBag.IsAuthenticated = isAuthenticated;
                ViewBag.AuthToken = !string.IsNullOrEmpty(sessionToken) ? sessionToken : cookieToken;

                // Kiểm tra xem user đã từng mượn sách này chưa
                bool hasBorrowedBook = false;
                string? borrowStatus = null;
                bool canReview = false;
                if (isAuthenticated)
                {
                    try
                    {
                        var currentUser = await _authApiService.GetCurrentUserAsync();
                        if (currentUser != null)
                        {
                            var authToken = !string.IsNullOrEmpty(sessionToken) ? sessionToken : cookieToken;
                            if (!string.IsNullOrEmpty(authToken))
                            {
                                hasBorrowedBook = await _bookApiService.HasUserBorrowedBookAsync(currentUser.Id, id, authToken);

                                // Nếu đã từng mượn, lấy trạng thái mượn hiện tại và kiểm tra có thể đánh giá không
                                if (hasBorrowedBook)
                                {
                                    var borrowedBooks = await _bookApiService.GetBorrowedBooksAsync(currentUser.Id, authToken);
                                    var bookBorrow = borrowedBooks.FirstOrDefault(b => b.BookId == id);
                                    if (bookBorrow != null)
                                    {
                                        borrowStatus = bookBorrow.Status;
                                    }
                                    else
                                    {
                                        // Kiểm tra lịch sử mượn
                                        var borrowHistory = await _bookApiService.GetBorrowHistoryAsync(currentUser.Id, authToken);
                                        var historyBorrow = borrowHistory.FirstOrDefault(b => b.BookId == id);
                                        if (historyBorrow != null)
                                        {
                                            borrowStatus = historyBorrow.Status;
                                        }
                                    }

                                    // Kiểm tra xem user có thể đánh giá sách này không (đã từng trả hoặc hủy)
                                    var borrowHistoryForReview = await _bookApiService.GetBorrowHistoryAsync(currentUser.Id, authToken);
                                    canReview = borrowHistoryForReview.Any(b => b.BookId == id && (b.Status == "Returned" || b.Status == "Cancelled"));
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not check if user has borrowed book {BookId}", id);
                        hasBorrowedBook = false;
                        borrowStatus = null;
                        canReview = false;
                    }
                }
                ViewBag.HasBorrowedBook = hasBorrowedBook;
                ViewBag.BorrowStatus = borrowStatus;
                ViewBag.CanReview = canReview; return View(book);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading book detail for ID: {BookId}", id);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
