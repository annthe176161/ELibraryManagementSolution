using ELibraryManagement.Api.DTOs;
using ELibraryManagement.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace ELibraryManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Route("odata/[controller]")]
    public class BookController : ControllerBase
    {
        private readonly IBookService _bookService;

        public BookController(IBookService bookService)
        {
            _bookService = bookService;
        }

        /// <summary>
        /// Lấy danh sách sách khả dụng trong thư viện
        /// </summary>
        [HttpGet("available")]
        [EnableQuery]
        public IActionResult GetAvailableBooks()
        {
            var books = _bookService.GetAvailableBooksQueryable();
            return Ok(books);
        }

        /// <summary>
        /// Lấy thông tin sách theo ID
        /// </summary>
        [HttpGet("{id}")]
        [EnableQuery]
        public async Task<IActionResult> GetBookById(int id)
        {
            var book = await _bookService.GetBookByIdAsync(id);
            if (book == null)
            {
                return NotFound($"Book with ID {id} not found.");
            }
            return Ok(book);
        }
    }
}
