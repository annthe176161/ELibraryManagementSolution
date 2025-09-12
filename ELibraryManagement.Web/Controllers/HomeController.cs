using ELibraryManagement.Web.Models;
using ELibraryManagement.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ELibraryManagement.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBookApiService _bookApiService;
        private readonly IAuthApiService _authApiService;

        public HomeController(ILogger<HomeController> logger, IBookApiService bookApiService, IAuthApiService authApiService)
        {
            _logger = logger;
            _bookApiService = bookApiService;
            _authApiService = authApiService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Books(string? search, string? category, string? author, string? sortBy, int page = 1, int pageSize = 12)
        {
            try
            {
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

                return View(books);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading books");
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

                // Truyền thông tin authentication vào View
                ViewBag.IsAuthenticated = _authApiService.IsAuthenticated();
                ViewBag.AuthToken = _authApiService.GetCurrentToken();

                return View(book);
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
