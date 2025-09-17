using ELibraryManagement.Api.Data;
using ELibraryManagement.Api.DTOs;
using ELibraryManagement.Api.Services.Interfaces;
using ELibraryManagement.Api.Services;
using ELibraryManagement.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ELibraryManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class BorrowController : ControllerBase
    {
        private readonly IBorrowService _borrowService;
        private readonly IBorrowStatusValidationService _validationService;
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public BorrowController(
            IBorrowService borrowService,
            IBorrowStatusValidationService validationService,
            ApplicationDbContext context,
            IEmailService emailService)
        {
            _borrowService = borrowService;
            _validationService = validationService;
            _context = context;
            _emailService = emailService;
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
        /// Gia hạn sách (User tự gia hạn)
        /// </summary>
        [HttpPost("{id}/extend")]
        [Authorize]
        public async Task<IActionResult> ExtendBorrow(int id, [FromBody] ExtendBorrowRequestDto? request = null)
        {
            try
            {
                // Kiểm tra người dùng có quyền gia hạn sách này không
                var borrowRecord = await _borrowService.GetBorrowRecordByIdAsync(id);
                if (borrowRecord == null)
                {
                    return NotFound(new { message = "Không tìm thấy bản ghi mượn sách" });
                }

                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (borrowRecord.UserId != currentUserId && !User.IsInRole("Admin"))
                {
                    return Forbid("Bạn không có quyền gia hạn sách này");
                }

                var result = await _borrowService.ExtendBorrowAsync(id, request?.Reason);

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
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
        /// Gia hạn ngày trả sách - Chỉ dành cho Admin
        /// </summary>
        [HttpPut("admin/{id}/extend")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExtendDueDate(int id, [FromBody] ExtendDueDateDto extendDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _borrowService.ExtendDueDateAsync(id, extendDto.NewDueDate);
                if (result)
                {
                    return Ok(new { message = "Gia hạn thành công" });
                }
                return NotFound(new { message = "Không tìm thấy borrow record" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Gửi thông báo nhắc nhở - Chỉ dành cho Admin
        /// </summary>
        [HttpPost("admin/{id}/remind")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendReminder(int id)
        {
            try
            {
                var result = await _borrowService.SendReminderAsync(id);
                if (result)
                {
                    return Ok(new { message = "Đã gửi thông báo nhắc nhở" });
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
        /// </summary>
        [HttpGet("admin/{id}/allowed-transitions")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllowedTransitions(int id)
        {
            try
            {
                var borrowRecord = await _context.BorrowRecords.FindAsync(id);
                if (borrowRecord == null)
                {
                    return NotFound(new { message = "Borrow record not found" });
                }

                var allowedStatuses = _validationService.GetAllowedTransitions(borrowRecord.Status);
                var statusList = allowedStatuses.Select(status => new
                {
                    Value = status.ToString(),
                    DisplayName = status switch
                    {
                        BorrowStatus.Requested => "Chờ duyệt",
                        BorrowStatus.Borrowed => "Đang mượn",
                        BorrowStatus.Returned => "Đã trả",
                        BorrowStatus.Lost => "Mất sách",
                        BorrowStatus.Damaged => "Hư hỏng",
                        BorrowStatus.Cancelled => "Đã hủy",
                        _ => status.ToString()
                    },
                    IsFinal = _validationService.IsFinalStatus(status)
                });

                return Ok(new
                {
                    currentStatus = borrowRecord.Status.ToString(),
                    currentStatusDisplay = borrowRecord.Status switch
                    {
                        BorrowStatus.Requested => "Chờ duyệt",
                        BorrowStatus.Borrowed => "Đang mượn",
                        BorrowStatus.Returned => "Đã trả",
                        BorrowStatus.Lost => "Mất sách",
                        BorrowStatus.Damaged => "Hư hỏng",
                        BorrowStatus.Cancelled => "Đã hủy",
                        _ => borrowRecord.Status.ToString()
                    },
                    isFinalStatus = _validationService.IsFinalStatus(borrowRecord.Status),
                    allowedTransitions = statusList
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Gửi email nhắc nhở trả sách thủ công - Chỉ dành cho Admin
        /// </summary>
        [HttpPost("admin/{id}/send-reminder")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendDueReminder(int id)
        {
            try
            {
                var borrowRecord = await _context.BorrowRecords
                    .Include(br => br.User)
                    .Include(br => br.Book)
                    .FirstOrDefaultAsync(br => br.Id == id);

                if (borrowRecord == null)
                {
                    return NotFound(new { message = "Không tìm thấy bản ghi mượn sách" });
                }

                if (borrowRecord.Status != BorrowStatus.Borrowed || borrowRecord.ReturnDate != null)
                {
                    return BadRequest(new { message = "Sách này không đang trong trạng thái mượn" });
                }

                var daysLeft = (borrowRecord.DueDate.Date - DateTime.UtcNow.Date).Days;
                var userName = $"{borrowRecord.User.FirstName} {borrowRecord.User.LastName}".Trim();

                if (string.IsNullOrEmpty(userName))
                {
                    userName = borrowRecord.User.Email?.Split('@')[0] ?? "Unknown User";
                }

                if (string.IsNullOrEmpty(borrowRecord.User.Email))
                {
                    return BadRequest(new { message = "Email người dùng không hợp lệ" });
                }

                var emailSent = await _emailService.SendBookDueReminderAsync(
                    borrowRecord.User.Email,
                    userName,
                    borrowRecord.Book.Title,
                    borrowRecord.DueDate,
                    daysLeft,
                    borrowRecord.CanExtend
                );

                if (emailSent)
                {
                    // Cập nhật notes để ghi lại việc gửi email thủ công
                    var reminderNote = $"MANUAL_REMINDER_{DateTime.UtcNow:yyyy-MM-dd_HH:mm} - Email nhắc nhở gửi thủ công bởi admin";
                    borrowRecord.Notes = $"{borrowRecord.Notes}\n{reminderNote}";
                    borrowRecord.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    return Ok(new
                    {
                        message = "Đã gửi email nhắc nhở thành công",
                        emailSent = true,
                        recipient = borrowRecord.User.Email
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        message = "Không thể gửi email nhắc nhở",
                        emailSent = false
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách sách sắp hết hạn - Chỉ dành cho Admin
        /// </summary>
        [HttpGet("admin/due-soon")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetBooksDueSoon([FromQuery] int days = 7)
        {
            try
            {
                var today = DateTime.UtcNow.Date;
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
                        daysLeft = (br.DueDate.Date - today).Days,
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
        /// Trigger manual email reminders for testing - Chỉ dành cho Admin
        /// </summary>
        [HttpPost("admin/trigger-reminders")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> TriggerReminders()
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var threeDaysFromNow = today.AddDays(3);
                var oneDayFromNow = today.AddDays(1);

                var borrowsNearingDue = await _context.BorrowRecords
                    .Include(br => br.User)
                    .Include(br => br.Book)
                    .Where(br => br.Status == BorrowStatus.Borrowed &&
                               br.ReturnDate == null &&
                               (br.DueDate.Date == threeDaysFromNow ||
                                br.DueDate.Date == oneDayFromNow ||
                                br.DueDate.Date == today))
                    .ToListAsync();

                var emailsSent = 0;
                var emailsFailed = 0;

                foreach (var borrow in borrowsNearingDue)
                {
                    try
                    {
                        var daysLeft = (borrow.DueDate.Date - today).Days;
                        var userName = $"{borrow.User.FirstName} {borrow.User.LastName}".Trim();

                        if (string.IsNullOrEmpty(userName))
                        {
                            userName = borrow.User.Email?.Split('@')[0] ?? "Unknown User";
                        }

                        if (string.IsNullOrEmpty(borrow.User.Email))
                        {
                            emailsFailed++;
                            continue;
                        }

                        var emailSent = await _emailService.SendBookDueReminderAsync(
                            borrow.User.Email,
                            userName,
                            borrow.Book.Title,
                            borrow.DueDate,
                            daysLeft,
                            borrow.CanExtend
                        );

                        if (emailSent)
                        {
                            emailsSent++;

                            // Mark email as sent
                            var reminderKey = $"MANUAL_TEST_{DateTime.UtcNow:yyyy-MM-dd_HH:mm}";
                            borrow.Notes = $"{borrow.Notes}\n{reminderKey} - Test reminder email sent";
                            borrow.UpdatedAt = DateTime.UtcNow;
                        }
                        else
                        {
                            emailsFailed++;
                        }
                    }
                    catch
                    {
                        emailsFailed++;
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = $"Đã gửi {emailsSent} email thành công, {emailsFailed} email thất bại",
                    totalFound = borrowsNearingDue.Count,
                    emailsSent = emailsSent,
                    emailsFailed = emailsFailed
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}