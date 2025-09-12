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

        public BorrowService(ApplicationDbContext context)
        {
            _context = context;
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

            if (Enum.TryParse<BorrowStatus>(updateDto.Status, out var status))
            {
                borrowRecord.Status = status;
                borrowRecord.Notes = updateDto.Notes;
                borrowRecord.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }

            return false;
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
    }
}