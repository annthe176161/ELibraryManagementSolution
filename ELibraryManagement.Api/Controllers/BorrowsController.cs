using ELibraryManagement.Api.Data;
using ELibraryManagement.Api.DTOs;
using ELibraryManagement.Api.Services.Interfaces;
using ELibraryManagement.Api.Services;
using ELibraryManagement.Api.Models;
using Microsoft.AspNetCore.Authorization;
using ELibraryManagement.Api.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ELibraryManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class BorrowsController : ControllerBase
    {
        private readonly IBorrowService _borrowService;
        private readonly IBorrowStatusValidationService _validationService;
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IUserStatusService _userStatusService;

        public BorrowsController(
            IBorrowService borrowService,
            IBorrowStatusValidationService validationService,
            ApplicationDbContext context,
            IEmailService emailService,
            IUserStatusService userStatusService)
        {
            _borrowService = borrowService;
            _validationService = validationService;
            _context = context;
            _emailService = emailService;
            _userStatusService = userStatusService;
        }

        /// <summary>
        /// Lấy tất cả borrow records - Chỉ dành cho Admin
        /// </summary>
        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllBorrowRecords()
        {
            try
            {
                var borrowRecords = await _borrowService.GetAllBorrowRecordsAsync();
                return Ok(borrowRecords);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy borrow record theo ID - Chỉ dành cho Admin
        /// </summary>
        [HttpGet("admin/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetBorrowRecordById(int id)
        {
            try
            {
                var borrowRecord = await _borrowService.GetBorrowRecordByIdAsync(id);
                if (borrowRecord == null)
                {
                    return NotFound(new { message = $"Borrow record with ID {id} not found" });
                }
                return Ok(borrowRecord);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật trạng thái borrow record - Chỉ dành cho Admin
        /// </summary>
        [HttpPut("admin/{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateBorrowStatus(int id, [FromBody] UpdateBorrowStatusDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _borrowService.UpdateBorrowStatusAsync(id, updateDto);
                if (result)
                {
                    return Ok(new { message = "Cập nhật trạng thái thành công" });
                }
                return NotFound(new { message = "Không tìm thấy borrow record" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Xác nhận trả sách - Chỉ dành cho Admin
        /// </summary>
        [HttpPost("admin/{id}/return")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ConfirmReturn(int id)
        {
            try
            {
                var result = await _borrowService.ConfirmReturnAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách trạng thái có thể chuyển từ trạng thái hiện tại - Chỉ dành cho Admin
        /// <summary>
        /// Lấy danh sách sách sắp hết hạn - Chỉ dành cho Admin
        /// </summary>
        [HttpGet("admin/due-soon")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetBooksDueSoon([FromQuery] int days = 7)
        {
            try
            {
                var today = DateTime.UtcNow.ToVietnamTime().Date;
                var targetDate = today.AddDays(days);

                var dueSoonBooks = await _context.BorrowRecords
                    .Include(br => br.User)
                    .Include(br => br.Book)
                    .Where(br => br.Status == BorrowStatus.Borrowed &&
                               br.ReturnDate == null &&
                               br.DueDate.Date >= today &&
                               br.DueDate.Date <= targetDate)
                    .OrderBy(br => br.DueDate)
                    .Select(br => new
                    {
                        id = br.Id,
                        bookTitle = br.Book.Title,
                        userName = $"{br.User.FirstName} {br.User.LastName}".Trim(),
                        userEmail = br.User.Email,
                        borrowDate = br.BorrowDate,
                        dueDate = br.DueDate,
                        daysLeft = (br.DueDate.ToVietnamTime().Date - today).Days,
                        canExtend = br.CanExtend,
                        extensionCount = br.ExtensionCount,
                        isOverdue = br.IsOverdue,
                        lastReminderSent = br.Notes != null && br.Notes.Contains("REMINDER_")
                            ? "Đã gửi" : "Chưa gửi"
                    })
                    .ToListAsync();

                return Ok(dueSoonBooks);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy lịch sử mượn sách của một user - Chỉ dành cho Admin
        /// </summary>
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserBorrowHistory(string userId)
        {
            try
            {
                var borrowRecords = await _context.BorrowRecords
                    .Include(br => br.Book)
                    .Include(br => br.User)
                    .Where(br => br.UserId == userId)
                    .OrderByDescending(br => br.CreatedAt)
                    .ToListAsync();

                var result = borrowRecords.Select(br => new
                {
                    id = br.Id,
                    bookId = br.BookId,
                    bookTitle = br.Book.Title,
                    bookAuthor = br.Book.Author,
                    bookCoverUrl = br.Book.CoverImageUrl,
                    borrowDate = br.BorrowDate,
                    confirmedDate = br.ConfirmedDate,
                    dueDate = br.DueDate,
                    returnDate = br.ReturnDate,
                    status = br.Status.ToString(),
                    notes = br.Notes
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Admin phê duyệt yêu cầu mượn sách - Chỉ dành cho Admin
        /// </summary>
        [HttpPost("approve/{borrowRecordId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveBorrowRequest(int borrowRecordId)
        {
            try
            {
                var borrowRecord = await _context.BorrowRecords
                    .Include(br => br.Book)
                    .FirstOrDefaultAsync(br => br.Id == borrowRecordId);

                if (borrowRecord == null)
                {
                    return NotFound(new { message = "Không tìm thấy yêu cầu mượn sách." });
                }

                if (borrowRecord.Status != BorrowStatus.Requested)
                {
                    return BadRequest(new { message = "Yêu cầu này đã được xử lý." });
                }

                // Check if book is still available
                var currentBorrowedCount = await _context.BorrowRecords
                    .CountAsync(br => br.BookId == borrowRecord.BookId && br.Status == BorrowStatus.Borrowed);

                if (currentBorrowedCount >= borrowRecord.Book.Quantity)
                {
                    return BadRequest(new { message = "Sách không còn khả dụng." });
                }

                // Check if user is allowed to borrow (respect user's borrow limit, block status and outstanding fines)
                var userStatus = await _userStatusService.GetUserStatusAsync(borrowRecord.UserId);

                // Compute live count of currently borrowed books for the user
                var userLiveBorrowedCount = await _context.BorrowRecords
                    .CountAsync(br => br.UserId == borrowRecord.UserId && br.Status == BorrowStatus.Borrowed);

                if (userStatus.AccountStatus == UserAccountStatus.Blocked)
                {
                    return BadRequest(new { message = $"Tài khoản người dùng đã bị khóa. Lý do: {userStatus.BlockReason}" });
                }

                if (userLiveBorrowedCount >= userStatus.MaxBorrowLimit)
                {
                    return BadRequest(new { message = $"Người dùng đã đạt giới hạn mượn sách ({userStatus.MaxBorrowLimit} cuốn)." });
                }

                if (userStatus.TotalOutstandingFines > 50000)
                {
                    return BadRequest(new { message = $"Người dùng có khoản phạt chưa thanh toán là {userStatus.TotalOutstandingFines:N0} VND. Vui lòng thanh toán trước." });
                }

                // Update status to Borrowed and decrease available quantity
                borrowRecord.Status = BorrowStatus.Borrowed;
                borrowRecord.ConfirmedDate = DateTime.UtcNow;

                // Update book's available quantity
                borrowRecord.Book.AvailableQuantity--;

                await _context.SaveChangesAsync();

                // Increment user's current borrow count now that the request has been approved
                // and the status changed to Borrowed. This keeps CurrentBorrowCount in sync
                // with actual borrowed items.
                try
                {
                    await _userStatusService.IncrementBorrowCountAsync(borrowRecord.UserId);
                }
                catch
                {
                    // Log or ignore - we don't want to fail the approval if user status update fails,
                    // but this should be investigated if it happens.
                }

                return Ok(new { message = "Đã phê duyệt yêu cầu mượn sách thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("admin/process-overdue")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ProcessOverdueBooks([FromServices] IOverdueProcessingService overdueService)
        {
            try
            {
                var processedCount = await overdueService.ProcessOverdueBooksAsync();
                return Ok(new
                {
                    message = $"Đã xử lý {processedCount} borrow records quá hạn",
                    processedCount = processedCount
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}