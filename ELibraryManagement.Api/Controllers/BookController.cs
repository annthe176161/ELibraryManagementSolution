using ELibraryManagement.Api.DTOs;
using ELibraryManagement.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ELibraryManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
        public async Task<IActionResult> GetAvailableBooks()
        {
            var books = await _bookService.GetAvailableBooksAsync();
            return Ok(books);
        }

        /// <summary>
        /// Lấy thông tin sách theo ID
        /// </summary>
        [HttpGet("{id}")]
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
