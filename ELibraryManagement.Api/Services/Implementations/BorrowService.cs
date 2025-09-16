using ELibraryManagement.Api.Data;
using ELibraryManagement.Api.DTOs;
using ELibraryManagement.Api.Models;
using ELibraryManagement.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ELibraryManagement.Api.Services.Implementations
{
    public class BorrowService : IBorrowService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBorrowStatusValidationService _validationService;

        public BorrowService(ApplicationDbContext context, IBorrowStatusValidationService validationService)
        {
            _context = context;
            _validationService = validationService;
        }

        public async Task<IEnumerable<BorrowRecordDto>> GetAllBorrowRecordsAsync()
        {
            return await _context.BorrowRecords
                .Include(br => br.Book)
                .Include(br => br.User)
                .Where(br => !br.Book.IsDeleted)
                .OrderByDescending(br => br.BorrowDate)
                .Select(br => new BorrowRecordDto
                {
                    Id = br.Id,
                    BookId = br.BookId,
                    BookTitle = br.Book.Title,
                    BookAuthor = br.Book.Author,
                    BookCoverUrl = br.Book.CoverImageUrl ?? "",
                    UserId = br.UserId,
                    UserName = $"{br.User.FirstName} {br.User.LastName}",
                    UserEmail = br.User.Email ?? "",
                    BorrowDate = br.BorrowDate,
                    DueDate = br.DueDate,
                    ReturnDate = br.ReturnDate,
                    Status = br.Status.ToString(),
                    Notes = br.Notes,
                    FineAmount = _context.Fines
                        .Where(f => f.BorrowRecordId == br.Id && f.Status == FineStatus.Pending)
                        .Sum(f => (decimal?)f.Amount)
                })
                .ToListAsync();
        }

        public async Task<BorrowRecordDto?> GetBorrowRecordByIdAsync(int id)
        {
            return await _context.BorrowRecords
                .Include(br => br.Book)
                .Include(br => br.User)
                .Where(br => br.Id == id && !br.Book.IsDeleted)
                .Select(br => new BorrowRecordDto
                {
                    Id = br.Id,
                    BookId = br.BookId,
                    BookTitle = br.Book.Title,
                    BookAuthor = br.Book.Author,
                    BookCoverUrl = br.Book.CoverImageUrl ?? "",
                    UserId = br.UserId,
                    UserName = $"{br.User.FirstName} {br.User.LastName}",
                    UserEmail = br.User.Email ?? "",
                    BorrowDate = br.BorrowDate,
                    DueDate = br.DueDate,
                    ReturnDate = br.ReturnDate,
                    Status = br.Status.ToString(),
                    Notes = br.Notes,
                    FineAmount = _context.Fines
                        .Where(f => f.BorrowRecordId == br.Id && f.Status == FineStatus.Pending)
                        .Sum(f => (decimal?)f.Amount)
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateBorrowStatusAsync(int id, UpdateBorrowStatusDto updateDto)
        {
            var borrowRecord = await _context.BorrowRecords.FindAsync(id);
            if (borrowRecord == null)
                return false;

            if (Enum.TryParse<BorrowStatus>(updateDto.Status, out var newStatus))
            {
                // Validate trạng thái chuyển đổi
                if (!_validationService.CanTransition(borrowRecord.Status, newStatus))
                {
                    throw new InvalidOperationException(
                        _validationService.GetTransitionErrorMessage(borrowRecord.Status, newStatus));
                }

                borrowRecord.Status = newStatus;
                borrowRecord.Notes = updateDto.Notes;
                borrowRecord.UpdatedAt = DateTime.UtcNow;

                // Cập nhật các trường liên quan dựa trên trạng thái mới
                UpdateRelatedFields(borrowRecord, newStatus);

                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        private void UpdateRelatedFields(BorrowRecord borrowRecord, BorrowStatus newStatus)
        {
            switch (newStatus)
            {
                case BorrowStatus.Borrowed:
                    // Khi chuyển sang Borrowed, cập nhật ngày xác nhận
                    borrowRecord.ConfirmedDate = DateTime.UtcNow;
                    break;

                case BorrowStatus.Returned:
                    // Khi trả sách, cập nhật ngày trả
                    borrowRecord.ReturnDate = DateTime.UtcNow;
                    break;

                case BorrowStatus.Cancelled:
                    // Khi hủy, có thể xóa ngày trả nếu có
                    borrowRecord.ReturnDate = null;
                    break;

                case BorrowStatus.Lost:
                case BorrowStatus.Damaged:
                    // Có thể tạo fine record tự động
                    // Logic này có thể được mở rộng sau
                    break;
            }
        }

        public async Task<bool> UpdateBorrowNotesAsync(int id, string? notes)
        {
            var borrowRecord = await _context.BorrowRecords.FindAsync(id);
            if (borrowRecord == null)
                return false;

            borrowRecord.Notes = notes;
            borrowRecord.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExtendDueDateAsync(int id, DateTime newDueDate)
        {
            var borrowRecord = await _context.BorrowRecords.FindAsync(id);
            if (borrowRecord == null || borrowRecord.Status != BorrowStatus.Borrowed)
                return false;

            borrowRecord.DueDate = newDueDate;
            borrowRecord.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SendReminderAsync(int id)
        {
            var borrowRecord = await _context.BorrowRecords
                .Include(br => br.User)
                .Include(br => br.Book)
                .FirstOrDefaultAsync(br => br.Id == id);

            if (borrowRecord == null)
                return false;

            // Here you would implement email sending logic
            // For now, we'll just log it or add a note
            borrowRecord.Notes = $"{borrowRecord.Notes}\nReminder sent on {DateTime.UtcNow:yyyy-MM-dd HH:mm}";
            borrowRecord.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ReturnBookResponseDto> ConfirmReturnAsync(int id)
        {
            var borrowRecord = await _context.BorrowRecords
                .Include(br => br.Book)
                .FirstOrDefaultAsync(br => br.Id == id);

            if (borrowRecord == null)
            {
                return new ReturnBookResponseDto
                {
                    Success = false,
                    Message = "Không tìm thấy record mượn sách"
                };
            }

            if (borrowRecord.Status == BorrowStatus.Returned)
            {
                return new ReturnBookResponseDto
                {
                    Success = false,
                    Message = "Sách đã được trả"
                };
            }

            var returnDate = DateTime.UtcNow;
            var isOverdue = returnDate > borrowRecord.DueDate;
            decimal? fineAmount = null;

            // Calculate fine if overdue
            if (isOverdue)
            {
                var overdueDays = (returnDate - borrowRecord.DueDate).Days;
                var finePerDay = 5000m; // 5000 VND per day
                fineAmount = overdueDays * finePerDay;

                // Create fine record
                var fine = new Fine
                {
                    BorrowRecordId = borrowRecord.Id,
                    Amount = fineAmount.Value,
                    Reason = $"Overdue return - {overdueDays} days late",
                    Status = FineStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Fines.Add(fine);
            }

            // Update borrow record
            borrowRecord.ReturnDate = returnDate;
            borrowRecord.Status = BorrowStatus.Returned;
            borrowRecord.UpdatedAt = DateTime.UtcNow;

            // Increase available quantity
            borrowRecord.Book.AvailableQuantity++;

            await _context.SaveChangesAsync();

            return new ReturnBookResponseDto
            {
                Success = true,
                BorrowRecordId = borrowRecord.Id,
                BookId = borrowRecord.BookId,
                BookTitle = borrowRecord.Book.Title,
                UserId = borrowRecord.UserId,
                ReturnDate = returnDate,
                FineAmount = fineAmount,
                Message = isOverdue ? $"Sách được trả thành công. Phí phạt: {fineAmount:N0} VND" : "Sách được trả thành công."
            };
        }

        public async Task<IEnumerable<BorrowRecordDto>> GetOverdueBorrowsAsync()
        {
            var currentDate = DateTime.UtcNow;
            return await _context.BorrowRecords
                .Include(br => br.Book)
                .Include(br => br.User)
                .Where(br => !br.Book.IsDeleted &&
                           br.Status == BorrowStatus.Borrowed &&
                           br.DueDate < currentDate)
                .OrderBy(br => br.DueDate)
                .Select(br => new BorrowRecordDto
                {
                    Id = br.Id,
                    BookId = br.BookId,
                    BookTitle = br.Book.Title,
                    BookAuthor = br.Book.Author,
                    BookCoverUrl = br.Book.CoverImageUrl ?? "",
                    UserId = br.UserId,
                    UserName = $"{br.User.FirstName} {br.User.LastName}",
                    UserEmail = br.User.Email ?? "",
                    BorrowDate = br.BorrowDate,
                    DueDate = br.DueDate,
                    ReturnDate = br.ReturnDate,
                    Status = br.Status.ToString(),
                    Notes = br.Notes
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<BorrowRecordDto>> GetBorrowsByStatusAsync(string status)
        {
            if (!Enum.TryParse<BorrowStatus>(status, out var borrowStatus))
                return new List<BorrowRecordDto>();

            return await _context.BorrowRecords
                .Include(br => br.Book)
                .Include(br => br.User)
                .Where(br => !br.Book.IsDeleted && br.Status == borrowStatus)
                .OrderByDescending(br => br.BorrowDate)
                .Select(br => new BorrowRecordDto
                {
                    Id = br.Id,
                    BookId = br.BookId,
                    BookTitle = br.Book.Title,
                    BookAuthor = br.Book.Author,
                    BookCoverUrl = br.Book.CoverImageUrl ?? "",
                    UserId = br.UserId,
                    UserName = $"{br.User.FirstName} {br.User.LastName}",
                    UserEmail = br.User.Email ?? "",
                    BorrowDate = br.BorrowDate,
                    DueDate = br.DueDate,
                    ReturnDate = br.ReturnDate,
                    Status = br.Status.ToString(),
                    Notes = br.Notes
                })
                .ToListAsync();
        }

        public async Task<ExtendBorrowResponseDto> ExtendBorrowAsync(int id, string? reason = null)
        {
            var borrowRecord = await _context.BorrowRecords
                .Include(br => br.Book)
                .FirstOrDefaultAsync(br => br.Id == id);

            if (borrowRecord == null)
            {
                return new ExtendBorrowResponseDto
                {
                    Success = false,
                    Message = "Không tìm thấy bản ghi mượn sách."
                };
            }

            // Kiểm tra điều kiện gia hạn
            if (!borrowRecord.CanExtend)
            {
                var reason_msg = "";
                if (borrowRecord.Status != BorrowStatus.Borrowed)
                    reason_msg = "Sách chưa được mượn hoặc đã trả.";
                else if (borrowRecord.ReturnDate != null)
                    reason_msg = "Sách đã được trả.";
                else if (borrowRecord.ExtensionCount >= 2)
                    reason_msg = "Đã hết số lần gia hạn (tối đa 2 lần).";
                else if (borrowRecord.IsOverdue)
                    reason_msg = "Không thể gia hạn sách quá hạn.";

                return new ExtendBorrowResponseDto
                {
                    Success = false,
                    BorrowRecordId = id,
                    BookTitle = borrowRecord.Book?.Title ?? "",
                    Message = $"Không thể gia hạn: {reason_msg}"
                };
            }

            // Thực hiện gia hạn
            var oldDueDate = borrowRecord.DueDate;
            var newDueDate = borrowRecord.DueDate.AddDays(14); // Gia hạn 14 ngày

            borrowRecord.DueDate = newDueDate;
            borrowRecord.ExtensionCount++;
            borrowRecord.LastExtensionDate = DateTime.UtcNow;
            borrowRecord.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(reason))
            {
                borrowRecord.Notes = (borrowRecord.Notes ?? "") +
                    $"\n[{DateTime.UtcNow:yyyy-MM-dd HH:mm}] Gia hạn lần {borrowRecord.ExtensionCount}: {reason}";
            }

            try
            {
                await _context.SaveChangesAsync();

                return new ExtendBorrowResponseDto
                {
                    Success = true,
                    BorrowRecordId = id,
                    BookTitle = borrowRecord.Book?.Title ?? "",
                    OldDueDate = oldDueDate,
                    NewDueDate = newDueDate,
                    ExtensionCount = borrowRecord.ExtensionCount,
                    Message = $"Gia hạn thành công! Ngày trả mới: {newDueDate:dd/MM/yyyy}. Còn lại {2 - borrowRecord.ExtensionCount} lần gia hạn."
                };
            }
            catch (Exception ex)
            {
                return new ExtendBorrowResponseDto
                {
                    Success = false,
                    BorrowRecordId = id,
                    BookTitle = borrowRecord.Book?.Title ?? "",
                    Message = $"Lỗi khi gia hạn: {ex.Message}"
                };
            }
        }
    }
}