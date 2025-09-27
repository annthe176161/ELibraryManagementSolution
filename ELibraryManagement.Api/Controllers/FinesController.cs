using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ELibraryManagement.Api.Data;
using ELibraryManagement.Api.Models;
using ELibraryManagement.Api.Services.Interfaces;
using System.Security.Claims;

namespace ELibraryManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class FinesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FinesController> _logger;
        private readonly IUserStatusService _userStatusService;

        public FinesController(ApplicationDbContext context, ILogger<FinesController> logger, IUserStatusService userStatusService)
        {
            _context = context;
            _logger = logger;
            _userStatusService = userStatusService;
        }

        /// <summary>
        /// Lấy tất cả phạt - Chỉ dành cho Admin
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllFines([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? status = null, [FromQuery] string? search = null)
        {
            try
            {
                var query = _context.Fines
                    .Include(f => f.User)
                    .Include(f => f.BorrowRecord)
                        .ThenInclude(br => br!.Book)
                    .AsQueryable();

                // Filter by status
                if (!string.IsNullOrEmpty(status) && Enum.TryParse<FineStatus>(status, out var fineStatus))
                {
                    query = query.Where(f => f.Status == fineStatus);
                }

                // Search by user name or email
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(f => f.User.FullName.Contains(search) || f.User.Email!.Contains(search));
                }

                var totalCount = await query.CountAsync();
                var fines = await query
                    .OrderByDescending(f => f.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(f => new
                    {
                        id = f.Id,
                        userId = f.UserId,
                        userFullName = f.User.FullName,
                        userEmail = f.User.Email,
                        borrowRecordId = f.BorrowRecordId,
                        bookTitle = f.BorrowRecord != null ? f.BorrowRecord.Book.Title : null,
                        amount = f.Amount,
                        reason = f.Reason,
                        description = f.Description,
                        status = f.Status.ToString(),
                        fineDate = f.FineDate,
                        paidDate = f.PaidDate,
                        dueDate = f.DueDate,
                        reminderCount = f.ReminderCount,
                        lastReminderDate = f.LastReminderDate,
                        escalationReason = f.EscalationReason,
                        escalationDate = f.EscalationDate,
                        createdAt = f.CreatedAt,
                        isOverdue = f.DueDate.HasValue && f.DueDate < DateTime.UtcNow && f.Status == FineStatus.Pending
                    })
                    .ToListAsync();

                return Ok(new
                {
                    fines,
                    totalCount,
                    page,
                    pageSize,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all fines");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy chi tiết phạt - Chỉ dành cho Admin
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetFineDetails(int id)
        {
            try
            {
                var fine = await _context.Fines
                    .Include(f => f.User)
                    .Include(f => f.BorrowRecord)
                        .ThenInclude(br => br!.Book)
                    .FirstOrDefaultAsync(f => f.Id == id);

                if (fine == null)
                {
                    return NotFound(new { message = "Không tìm thấy phạt" });
                }

                // Get action history
                var actionHistory = await _context.FineActionHistories
                    .Include(fah => fah.User)
                    .Where(fah => fah.FineId == id)
                    .OrderByDescending(fah => fah.ActionDate)
                    .Select(fah => new
                    {
                        id = fah.Id,
                        actionType = fah.ActionType.ToString(),
                        description = fah.Description,
                        amount = fah.Amount,
                        notes = fah.Notes,
                        actionDate = fah.ActionDate,
                        userFullName = fah.User.FullName,
                        userEmail = fah.User.Email
                    })
                    .ToListAsync();

                var result = new
                {
                    id = fine.Id,
                    userId = fine.UserId,
                    userFullName = fine.User.FullName,
                    userEmail = fine.User.Email,
                    borrowRecordId = fine.BorrowRecordId,
                    bookTitle = fine.BorrowRecord?.Book.Title,
                    bookAuthor = fine.BorrowRecord?.Book.Author,
                    amount = fine.Amount,
                    reason = fine.Reason,
                    description = fine.Description,
                    status = fine.Status.ToString(),
                    fineDate = fine.FineDate,
                    paidDate = fine.PaidDate,
                    dueDate = fine.DueDate,
                    reminderCount = fine.ReminderCount,
                    lastReminderDate = fine.LastReminderDate,
                    escalationReason = fine.EscalationReason,
                    escalationDate = fine.EscalationDate,
                    createdAt = fine.CreatedAt,
                    updatedAt = fine.UpdatedAt,
                    isOverdue = fine.DueDate.HasValue && fine.DueDate < DateTime.UtcNow && fine.Status == FineStatus.Pending,
                    actionHistory
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting fine details for ID: {FineId}", id);
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Tạo phạt mới - Chỉ dành cho Admin
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateFine([FromBody] CreateFineRequest request)
        {
            try
            {
                // Validate user exists
                var userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId);
                if (!userExists)
                {
                    return BadRequest(new { message = "Người dùng không tồn tại" });
                }

                // Validate borrow record if provided
                if (request.BorrowRecordId.HasValue)
                {
                    var borrowRecordExists = await _context.BorrowRecords.AnyAsync(br => br.Id == request.BorrowRecordId);
                    if (!borrowRecordExists)
                    {
                        return BadRequest(new { message = "Bản ghi mượn sách không tồn tại" });
                    }
                }

                var fine = new Fine
                {
                    UserId = request.UserId,
                    BorrowRecordId = request.BorrowRecordId,
                    Amount = request.Amount,
                    Reason = request.Reason,
                    Description = request.Description,
                    DueDate = request.DueDate ?? DateTime.UtcNow.AddDays(30), // Default 30 days
                    Status = FineStatus.Pending,
                    FineDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Fines.Add(fine);
                await _context.SaveChangesAsync();

                // Create action history
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(currentUserId))
                {
                    var actionHistory = new FineActionHistory
                    {
                        FineId = fine.Id,
                        UserId = currentUserId,
                        ActionType = FineActionType.ReminderSent,
                        Description = $"Tạo phạt mới: {request.Reason}",
                        Amount = request.Amount,
                        Notes = request.Description,
                        ActionDate = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.FineActionHistories.Add(actionHistory);
                    await _context.SaveChangesAsync();
                }

                return CreatedAtAction(nameof(GetFineDetails), new { id = fine.Id }, new { id = fine.Id, message = "Tạo phạt thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating fine");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật phạt - Chỉ dành cho Admin
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateFine(int id, [FromBody] UpdateFineRequest request)
        {
            try
            {
                var fine = await _context.Fines
                    .Include(f => f.BorrowRecord)
                        .ThenInclude(br => br!.Book)
                    .FirstOrDefaultAsync(f => f.Id == id);

                if (fine == null)
                {
                    return NotFound(new { message = "Không tìm thấy phạt" });
                }

                var oldStatus = fine.Status;
                var oldAmount = fine.Amount;

                fine.Amount = request.Amount;
                fine.Reason = request.Reason;
                fine.Description = request.Description;
                fine.DueDate = request.DueDate;
                fine.UpdatedAt = DateTime.UtcNow;

                if (request.Status.HasValue)
                {
                    fine.Status = request.Status.Value;
                    if (request.Status.Value == FineStatus.Paid && !fine.PaidDate.HasValue)
                    {
                        fine.PaidDate = DateTime.UtcNow;
                    }

                    // *** FIX: Handle BorrowRecord and CurrentBorrowCount when status changes to Paid ***
                    if (request.Status.Value == FineStatus.Paid && oldStatus != FineStatus.Paid)
                    {
                        // Update borrow record status if exists (payment means book was returned)
                        if (fine.BorrowRecord != null && fine.BorrowRecord.Status != BorrowStatus.Returned)
                        {
                            _logger.LogInformation("📚 Updating BorrowRecord via UpdateFine: ID={BorrowRecordId}, CurrentStatus={CurrentStatus}",
                                fine.BorrowRecord.Id, fine.BorrowRecord.Status);

                            fine.BorrowRecord.Status = BorrowStatus.Returned;
                            fine.BorrowRecord.ReturnDate = DateTime.UtcNow;
                            fine.BorrowRecord.UpdatedAt = DateTime.UtcNow;

                            // Update book available quantity
                            if (fine.BorrowRecord.Book != null)
                            {
                                fine.BorrowRecord.Book.AvailableQuantity++;
                                fine.BorrowRecord.Book.UpdatedAt = DateTime.UtcNow;

                                _logger.LogInformation("✅ Increased book {BookId} available quantity when fine {FineId} status updated to Paid",
                                    fine.BorrowRecord.Book.Id, fine.Id);
                            }

                            // Decrement user's CurrentBorrowCount when book is returned
                            _logger.LogInformation("👤 Decrementing CurrentBorrowCount for user: {UserId} (UpdateFine to Paid)", fine.UserId);
                            await _userStatusService.DecrementBorrowCountAsync(fine.UserId);
                            _logger.LogInformation("✅ Decremented CurrentBorrowCount for user {UserId} when fine {FineId} status updated to Paid", fine.UserId, fine.Id);
                        }
                    }
                }

                await _context.SaveChangesAsync();

                // Create action history
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(currentUserId))
                {
                    var description = "Cập nhật phạt";
                    if (oldStatus != fine.Status)
                    {
                        description += $" - Thay đổi trạng thái từ {oldStatus} thành {fine.Status}";
                    }
                    if (oldAmount != fine.Amount)
                    {
                        description += $" - Thay đổi số tiền từ {oldAmount:N0} VND thành {fine.Amount:N0} VND";
                    }

                    var actionHistory = new FineActionHistory
                    {
                        FineId = fine.Id,
                        UserId = currentUserId,
                        ActionType = fine.Status == FineStatus.Paid ? FineActionType.PaymentReceived : FineActionType.ReminderSent,
                        Description = description,
                        Amount = fine.Amount,
                        Notes = request.Notes,
                        ActionDate = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.FineActionHistories.Add(actionHistory);
                    await _context.SaveChangesAsync();
                }

                return Ok(new { message = "Cập nhật phạt thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating fine with ID: {FineId}", id);
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Đánh dấu phạt đã thanh toán - Chỉ dành cho Admin
        /// </summary>
        [HttpPost("{id}/pay")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MarkFineAsPaid(int id, [FromBody] PayFineRequest request)
        {
            try
            {
                _logger.LogInformation("🎯 Starting MarkFineAsPaid for Fine ID: {FineId}", id);

                var fine = await _context.Fines
                    .Include(f => f.BorrowRecord)
                        .ThenInclude(br => br!.Book)
                    .FirstOrDefaultAsync(f => f.Id == id);

                if (fine == null)
                {
                    _logger.LogWarning("❌ Fine not found with ID: {FineId}", id);
                    return NotFound(new { message = "Không tìm thấy phạt" });
                }

                _logger.LogInformation("✅ Found fine: ID={FineId}, Status={Status}, BorrowRecordId={BorrowRecordId}",
                    fine.Id, fine.Status, fine.BorrowRecordId);

                if (fine.Status == FineStatus.Paid)
                {
                    _logger.LogWarning("⚠️ Fine already paid: {FineId}", id);
                    return BadRequest(new { message = "Phạt đã được thanh toán" });
                }

                // Update fine status
                fine.Status = FineStatus.Paid;
                fine.PaidDate = DateTime.UtcNow;
                fine.UpdatedAt = DateTime.UtcNow;

                _logger.LogInformation("📝 Updated fine status to Paid for ID: {FineId}", id);

                // Update borrow record status if exists (payment means book was returned)
                if (fine.BorrowRecord != null && fine.BorrowRecord.Status != BorrowStatus.Returned)
                {
                    _logger.LogInformation("📚 Updating BorrowRecord: ID={BorrowRecordId}, CurrentStatus={CurrentStatus}",
                        fine.BorrowRecord.Id, fine.BorrowRecord.Status);

                    fine.BorrowRecord.Status = BorrowStatus.Returned;
                    fine.BorrowRecord.ReturnDate = DateTime.UtcNow;
                    fine.BorrowRecord.UpdatedAt = DateTime.UtcNow;

                    // Update book available quantity
                    if (fine.BorrowRecord.Book != null)
                    {
                        _logger.LogInformation("📖 Updating Book: ID={BookId}, CurrentAvailable={CurrentAvailable}",
                            fine.BorrowRecord.Book.Id, fine.BorrowRecord.Book.AvailableQuantity);

                        fine.BorrowRecord.Book.AvailableQuantity++;
                        fine.BorrowRecord.Book.UpdatedAt = DateTime.UtcNow;

                        _logger.LogInformation("✅ Increased book {BookId} available quantity to {AvailableQuantity} when fine {FineId} was paid",
                            fine.BorrowRecord.Book.Id, fine.BorrowRecord.Book.AvailableQuantity, fine.Id);
                    }

                    // *** FIX: Decrement user's CurrentBorrowCount when book is returned ***
                    _logger.LogInformation("👤 Decrementing CurrentBorrowCount for user: {UserId}", fine.UserId);
                    await _userStatusService.DecrementBorrowCountAsync(fine.UserId);
                    _logger.LogInformation("✅ Decremented CurrentBorrowCount for user {UserId} when fine {FineId} was paid", fine.UserId, fine.Id);

                    _logger.LogInformation("✅ Updated BorrowRecord {BorrowRecordId} status to Returned when fine {FineId} was paid",
                        fine.BorrowRecord.Id, fine.Id);
                }

                _logger.LogInformation("💾 Saving changes to database...");
                await _context.SaveChangesAsync();
                _logger.LogInformation("✅ Database changes saved successfully");

                // Create action history
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(currentUserId))
                {
                    var actionHistory = new FineActionHistory
                    {
                        FineId = fine.Id,
                        UserId = currentUserId,
                        ActionType = FineActionType.PaymentReceived,
                        Description = $"Đánh dấu phạt đã thanh toán - Số tiền: {fine.Amount:N0} VND",
                        Amount = fine.Amount,
                        Notes = request.Notes,
                        ActionDate = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.FineActionHistories.Add(actionHistory);
                    await _context.SaveChangesAsync();
                }

                var returnMessage = "Đã đánh dấu phạt là đã thanh toán";
                if (fine.BorrowRecord != null)
                {
                    returnMessage += " và cập nhật trạng thái sách đã trả";
                }

                return Ok(new { message = returnMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking fine as paid for ID: {FineId}", id);
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Miễn phạt - Chỉ dành cho Admin
        /// </summary>
        [HttpPost("{id}/waive")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> WaiveFine(int id, [FromBody] WaiveFineRequest request)
        {
            try
            {
                var fine = await _context.Fines
                    .Include(f => f.BorrowRecord)
                        .ThenInclude(br => br!.Book)
                    .FirstOrDefaultAsync(f => f.Id == id);

                if (fine == null)
                {
                    return NotFound(new { message = "Không tìm thấy phạt" });
                }

                if (fine.Status == FineStatus.Paid || fine.Status == FineStatus.Waived)
                {
                    return BadRequest(new { message = "Phạt đã được xử lý" });
                }

                fine.Status = FineStatus.Waived;
                fine.UpdatedAt = DateTime.UtcNow;

                // Update borrow record status if exists (waiving fine means book was returned)
                if (fine.BorrowRecord != null && fine.BorrowRecord.Status != BorrowStatus.Returned)
                {
                    _logger.LogInformation("📚 Updating BorrowRecord for waived fine: ID={BorrowRecordId}, CurrentStatus={CurrentStatus}",
                        fine.BorrowRecord.Id, fine.BorrowRecord.Status);

                    fine.BorrowRecord.Status = BorrowStatus.Returned;
                    fine.BorrowRecord.ReturnDate = DateTime.UtcNow;
                    fine.BorrowRecord.UpdatedAt = DateTime.UtcNow;

                    // Update book available quantity
                    if (fine.BorrowRecord.Book != null)
                    {
                        fine.BorrowRecord.Book.AvailableQuantity++;
                        fine.BorrowRecord.Book.UpdatedAt = DateTime.UtcNow;

                        _logger.LogInformation("✅ Increased book {BookId} available quantity when fine {FineId} was waived",
                            fine.BorrowRecord.Book.Id, fine.Id);
                    }

                    // *** FIX: Decrement user's CurrentBorrowCount when book is returned via waiving fine ***
                    _logger.LogInformation("👤 Decrementing CurrentBorrowCount for user: {UserId} (waived fine)", fine.UserId);
                    await _userStatusService.DecrementBorrowCountAsync(fine.UserId);
                    _logger.LogInformation("✅ Decremented CurrentBorrowCount for user {UserId} when fine {FineId} was waived", fine.UserId, fine.Id);
                }

                await _context.SaveChangesAsync();

                // Create action history
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(currentUserId))
                {
                    var actionHistory = new FineActionHistory
                    {
                        FineId = fine.Id,
                        UserId = currentUserId,
                        ActionType = FineActionType.FineWaived,
                        Description = $"Miễn phạt - Lý do: {request.Reason}",
                        Amount = fine.Amount,
                        Notes = request.Notes,
                        ActionDate = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.FineActionHistories.Add(actionHistory);
                    await _context.SaveChangesAsync();
                }

                return Ok(new { message = "Đã miễn phạt thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error waiving fine for ID: {FineId}", id);
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy phạt của một user cụ thể - Chỉ dành cho Admin
        /// </summary>
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserFines(string userId)
        {
            try
            {
                var fines = await _context.Fines
                    .Include(f => f.BorrowRecord)
                        .ThenInclude(br => br!.Book)
                    .Where(f => f.UserId == userId)
                    .OrderByDescending(f => f.CreatedAt)
                    .Select(f => new
                    {
                        id = f.Id,
                        borrowRecordId = f.BorrowRecordId,
                        bookTitle = f.BorrowRecord != null ? f.BorrowRecord.Book.Title : null,
                        amount = f.Amount,
                        reason = f.Reason,
                        description = f.Description,
                        status = f.Status.ToString(),
                        fineDate = f.FineDate,
                        paidDate = f.PaidDate,
                        dueDate = f.DueDate,
                        isOverdue = f.DueDate.HasValue && f.DueDate < DateTime.UtcNow && f.Status == FineStatus.Pending,
                        createdAt = f.CreatedAt
                    })
                    .ToListAsync();

                return Ok(fines);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting fines for user: {UserId}", userId);
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Thống kê phạt - Chỉ dành cho Admin
        /// </summary>
        [HttpGet("statistics")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetFineStatistics()
        {
            try
            {
                var totalFines = await _context.Fines.CountAsync();
                var pendingFines = await _context.Fines.CountAsync(f => f.Status == FineStatus.Pending);
                var paidFines = await _context.Fines.CountAsync(f => f.Status == FineStatus.Paid);
                var waivedFines = await _context.Fines.CountAsync(f => f.Status == FineStatus.Waived);
                // Consider fines that were created because a borrow is overdue (Reason contains "Quá hạn"
                // or the related BorrowRecord has status Overdue) as "Overdue" for administrative statistics.
                var overdueFines = await _context.Fines
                    .Include(f => f.BorrowRecord)
                    .Where(f => f.Status == FineStatus.Pending && (
                        (f.Reason != null && f.Reason.Contains("Quá hạn")) ||
                        (f.BorrowRecord != null && f.BorrowRecord.Status == BorrowStatus.Overdue)
                    ))
                    .CountAsync();

                var totalAmount = await _context.Fines.SumAsync(f => f.Amount);
                var paidAmount = await _context.Fines.Where(f => f.Status == FineStatus.Paid).SumAsync(f => f.Amount);
                var pendingAmount = await _context.Fines.Where(f => f.Status == FineStatus.Pending).SumAsync(f => f.Amount);

                return Ok(new
                {
                    totalFines,
                    pendingFines,
                    paidFines,
                    waivedFines,
                    overdueFines,
                    totalAmount,
                    paidAmount,
                    pendingAmount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting fine statistics");
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    // Request models
    public class CreateFineRequest
    {
        public string UserId { get; set; } = string.Empty;
        public int? BorrowRecordId { get; set; }
        public decimal Amount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
    }

    public class UpdateFineRequest
    {
        public decimal Amount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public FineStatus? Status { get; set; }
        public string? Notes { get; set; }
    }

    public class PayFineRequest
    {
        public string? Notes { get; set; }
    }

    public class WaiveFineRequest
    {
        public string Reason { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}