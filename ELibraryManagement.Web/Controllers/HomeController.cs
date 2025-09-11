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

        public HomeController(ILogger<HomeController> logger, IBookApiService bookApiService)
        {
            _logger = logger;
            _bookApiService = bookApiService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Books(string? search, string? category, string? author, decimal? minPrice, decimal? maxPrice, string? sortBy, int page = 1, int pageSize = 12)
        {
            try
            {
                var books = await _bookApiService.GetAvailableBooksAsync(search, category, author, minPrice, maxPrice, sortBy, page, pageSize);

                // Lấy danh sách categories và authors để hiển thị trong filter
                var categories = await _bookApiService.GetCategoriesAsync();
                var authors = await _bookApiService.GetAuthorsAsync();

                ViewBag.Categories = categories;
                ViewBag.Authors = authors;
                ViewBag.Search = search;
                ViewBag.Category = category;
                ViewBag.Author = author;
                ViewBag.MinPrice = minPrice;
                ViewBag.MaxPrice = maxPrice;
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
