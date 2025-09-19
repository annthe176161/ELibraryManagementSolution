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
        private readonly ICloudinaryService _cloudinaryService;

        public BookController(IBookService bookService, ICloudinaryService cloudinaryService)
        {
            _bookService = bookService;
            _cloudinaryService = cloudinaryService;
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
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _bookService.BorrowBookAsync(request);

                // Always return success for completed operations
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log the exception details for debugging
                Console.WriteLine($"Error in BorrowBook: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                // Return user-friendly error message
                return BadRequest(new
                {
                    message = ex.Message,
                    success = false
                });
            }
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
        /// Lấy toàn bộ lịch sử mượn sách của user - Yêu cầu đăng nhập
        /// </summary>
        [HttpGet("history/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetBorrowHistory(string userId)
        {
            var borrowHistory = await _bookService.GetBorrowHistoryByUserAsync(userId);
            return Ok(borrowHistory);
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

        /// <summary>
        /// Hủy yêu cầu mượn sách - Yêu cầu đăng nhập
        /// </summary>
        [HttpPost("cancel/{borrowRecordId}")]
        [Authorize]
        public async Task<IActionResult> CancelBorrowRequest(int borrowRecordId)
        {
            var result = await _bookService.CancelBorrowRequestAsync(borrowRecordId);

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
        /// Lấy chi tiết sách theo ID - Chỉ dành cho Admin
        /// </summary>
        [HttpGet("admin/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetBookDetailForAdmin(int id)
        {
            var book = await _bookService.GetBookByIdAsync(id);
            if (book == null)
            {
                return NotFound(new { message = $"Không tìm thấy sách với ID {id}" });
            }
            return Ok(book);
        }

        /// <summary>
        /// Tạo sách mới - Chỉ dành cho Admin
        /// </summary>
        [HttpPost("admin/create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateBook([FromBody] CreateBookDto createBookDto)
        {
            // Debug logging
            Console.WriteLine($"[BookController] CreateBook - Received CoverImageUrl: '{createBookDto.CoverImageUrl}'");
            Console.WriteLine($"[BookController] CreateBook - Full DTO: {System.Text.Json.JsonSerializer.Serialize(createBookDto)}");

            if (!ModelState.IsValid)
            {
                Console.WriteLine($"[BookController] CreateBook - ModelState invalid: {string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))}");
                return BadRequest(ModelState);
            }

            try
            {
                var book = await _bookService.CreateBookAsync(createBookDto);
                Console.WriteLine($"[BookController] CreateBook - Created book with ID: {book.Id}, CoverImageUrl: '{book.CoverImageUrl}'");
                return CreatedAtAction(nameof(GetBookById), new { id = book.Id }, book);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BookController] CreateBook - Exception: {ex.Message}");
                Console.WriteLine($"[BookController] CreateBook - Inner Exception: {ex.InnerException?.Message}");
                Console.WriteLine($"[BookController] CreateBook - Stack Trace: {ex.StackTrace}");

                // Return more detailed error message
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                return BadRequest(new { message = $"An error occurred while saving the entity changes. Details: {errorMessage}" });
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
                    return NotFound(new { message = $"Không tìm thấy sách với ID {id}" });
                }

                return Ok(new { message = "Xóa sách thành công" });
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
            try
            {
                var categories = await _bookService.GetAllCategoriesAsync();
                return Ok(new { success = true, data = categories });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Upload hình ảnh sách - Chỉ dành cho Admin
        /// </summary>
        [HttpPost("admin/upload-image")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadBookImage(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { message = "Vui lòng chọn file ảnh!" });
                }

                // Validate file type
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
                if (!allowedTypes.Contains(file.ContentType.ToLower()))
                {
                    return BadRequest(new { message = "Chỉ chấp nhận file ảnh (JPG, PNG, GIF, WebP)" });
                }

                // Validate file size (5MB)
                if (file.Length > 5 * 1024 * 1024)
                {
                    return BadRequest(new { message = "Kích thước file không được vượt quá 5MB" });
                }

                // Upload ảnh lên Cloudinary
                var imageUrl = await _cloudinaryService.UploadImageAsync(file, "books");
                if (imageUrl == null)
                {
                    return BadRequest(new { message = "Không thể tải lên hình ảnh" });
                }

                return Ok(new
                {
                    message = "Tải lên hình ảnh thành công",
                    imageUrl = imageUrl
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BookController] UploadBookImage - Exception: {ex.Message}");
                return StatusCode(500, new { message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        /// <summary>
        /// Sync AvailableQuantity for all books - Emergency admin tool
        /// </summary>
        [HttpPost("admin/sync-quantities")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SyncAvailableQuantities()
        {
            try
            {
                var result = await _bookService.SyncAvailableQuantitiesAsync();
                return Ok(new
                {
                    message = "Đã đồng bộ thành công",
                    updatedBooks = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
