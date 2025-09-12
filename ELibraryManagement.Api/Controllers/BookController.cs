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

        // Admin endpoints
        /// <summary>
        /// Lấy tất cả sách (bao gồm không khả dụng) - Chỉ dành cho Admin
        /// </summary>
        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllBooks()
        {
            var books = await _bookService.GetAllBooksAsync();
            return Ok(books);
        }

        /// <summary>
        /// Tạo sách mới - Chỉ dành cho Admin
        /// </summary>
        [HttpPost("admin/create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateBook([FromBody] CreateBookDto createBookDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var book = await _bookService.CreateBookAsync(createBookDto);
                return CreatedAtAction(nameof(GetBookById), new { id = book.Id }, book);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật thông tin sách - Chỉ dành cho Admin
        /// </summary>
        [HttpPut("admin/update")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateBook([FromBody] UpdateBookDto updateBookDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var book = await _bookService.UpdateBookAsync(updateBookDto);
                return Ok(book);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Xóa sách - Chỉ dành cho Admin
        /// </summary>
        [HttpDelete("admin/delete/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            try
            {
                var result = await _bookService.DeleteBookAsync(id);
                if (!result)
                {
                    return NotFound(new { message = $"Book with ID {id} not found" });
                }

                return Ok(new { message = "Book deleted successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy tất cả danh mục - Chỉ dành cho Admin
        /// </summary>
        [HttpGet("admin/categories")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _bookService.GetAllCategoriesAsync();
            return Ok(categories);
        }
    }
}
