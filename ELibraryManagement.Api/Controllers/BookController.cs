using ELibraryManagement.Api.DTOs;
using ELibraryManagement.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace ELibraryManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Route("odata/[controller]")]
    [Produces("application/json", "application/xml", "text/csv")]
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

        /// <summary>
        /// Đặt sách (mượn sách) - Yêu cầu đăng nhập
        /// </summary>
        [HttpPost("borrow")]
        [Authorize]
        public async Task<IActionResult> BorrowBook([FromBody] BorrowBookRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _bookService.BorrowBookAsync(request);

            if (string.IsNullOrEmpty(result.Message) || !result.Message.Contains("successfully"))
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách sách đã mượn của user - Yêu cầu đăng nhập
        /// </summary>
        [HttpGet("borrowed/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetBorrowedBooks(string userId)
        {
            var borrowedBooks = await _bookService.GetBorrowedBooksByUserAsync(userId);
            return Ok(borrowedBooks);
        }

        /// <summary>
        /// Trả sách - Yêu cầu đăng nhập
        /// </summary>
        [HttpPost("return/{borrowRecordId}")]
        [Authorize]
        public async Task<IActionResult> ReturnBook(int borrowRecordId)
        {
            var result = await _bookService.ReturnBookAsync(borrowRecordId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
