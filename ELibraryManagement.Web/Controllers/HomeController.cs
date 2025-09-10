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

        public async Task<IActionResult> Books()
        {
            try
            {
                var books = await _bookApiService.GetAvailableBooksAsync();
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
