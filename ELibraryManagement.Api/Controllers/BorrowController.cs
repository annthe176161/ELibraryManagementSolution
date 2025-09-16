using ELibraryManagement.Api.Data;
using ELibraryManagement.Api.DTOs;
using ELibraryManagement.Api.Services.Interfaces;
using ELibraryManagement.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        public BorrowController(
            IBorrowService borrowService,
            IBorrowStatusValidationService validationService,
            ApplicationDbContext context)
        {
            _borrowService = borrowService;
            _validationService = validationService;
            _context = context;
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
    }
}