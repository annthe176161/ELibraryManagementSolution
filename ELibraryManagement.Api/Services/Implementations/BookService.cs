using ELibraryManagement.Api.Data;
using ELibraryManagement.Api.DTOs;
using ELibraryManagement.Api.Models;
using ELibraryManagement.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ELibraryManagement.Api.Services.Implementations
{
    public class BookService : IBookService
    {
        private readonly ApplicationDbContext _context;

        public BookService(ApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<BookDto> GetAvailableBooksQueryable()
        {
            return _context.Books
                .Where(b => b.AvailableQuantity > 0 && !b.IsDeleted)
                .Include(b => b.BookCategories)
                .ThenInclude(bc => bc.Category)
                .Select(b => new BookDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Author = b.Author,
                    ISBN = b.ISBN,
                    Publisher = b.Publisher,
                    PublicationYear = b.PublicationYear,
                    Description = b.Description,
                    CoverImageUrl = b.CoverImageUrl,
                    Quantity = b.Quantity,
                    AvailableQuantity = b.AvailableQuantity,
                    Language = b.Language,
                    PageCount = b.PageCount,
                    AverageRating = (float)b.AverageRating,
                    RatingCount = b.RatingCount,
                    Categories = b.BookCategories.Select(bc => new CategoryDto
                    {
                        Id = bc.Category.Id,
                        Name = bc.Category.Name,
                        Description = bc.Category.Description,
                        Color = bc.Category.Color
                    }).ToList()
                });
        }

        public async Task<BookDto?> GetBookByIdAsync(int id)
        {
            var book = await _context.Books
                .Where(b => b.Id == id && !b.IsDeleted)
                .Include(b => b.BookCategories)
                .ThenInclude(bc => bc.Category)
                .FirstOrDefaultAsync();

            if (book == null) return null;

            return new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                ISBN = book.ISBN,
                Publisher = book.Publisher,
                PublicationYear = book.PublicationYear,
                Description = book.Description,
                CoverImageUrl = book.CoverImageUrl,
                Quantity = book.Quantity,
                AvailableQuantity = book.AvailableQuantity,
                Language = book.Language,
                PageCount = book.PageCount,
                AverageRating = (float)book.AverageRating,
                RatingCount = book.RatingCount,
                Categories = book.BookCategories.Select(bc => new CategoryDto
                {
                    Id = bc.Category.Id,
                    Name = bc.Category.Name,
                    Description = bc.Category.Description,
                    Color = bc.Category.Color
                }).ToList()
            };
        }

        public async Task<BorrowBookResponseDto> BorrowBookAsync(BorrowBookRequestDto request)
        {
            // Check if book exists and is available
            var book = await _context.Books.FindAsync(request.BookId);
            if (book == null)
            {
                throw new ArgumentException("Book not found.");
            }

            if (book.AvailableQuantity <= 0)
            {
                throw new InvalidOperationException("Book is not available for borrowing.");
            }

            // Check if user already has this book borrowed
            var existingBorrow = await _context.BorrowRecords
                .AnyAsync(br => br.UserId == request.UserId && br.BookId == request.BookId && br.Status == BorrowStatus.Borrowed);

            if (existingBorrow)
            {
                throw new InvalidOperationException("You have already borrowed this book.");
            }

            // Calculate due date (default 14 days if not provided)
            var dueDate = request.DueDate ?? DateTime.UtcNow.AddDays(14);

            // Create borrow record
            var borrowRecord = new BorrowRecord
            {
                UserId = request.UserId,
                BookId = request.BookId,
                BorrowDate = DateTime.UtcNow,
                DueDate = dueDate,
                Status = BorrowStatus.Borrowed,
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow
            };

            // Decrease available quantity
            book.AvailableQuantity--;

            // Save changes
            _context.BorrowRecords.Add(borrowRecord);
            await _context.SaveChangesAsync();

            return new BorrowBookResponseDto
            {
                BorrowRecordId = borrowRecord.Id,
                BookId = book.Id,
                BookTitle = book.Title,
                UserId = request.UserId,
                BorrowDate = borrowRecord.BorrowDate,
                DueDate = borrowRecord.DueDate,
                Status = borrowRecord.Status.ToString(),
                Message = "Book borrowed successfully."
            };
        }

        public async Task<IEnumerable<BorrowRecordDto>> GetBorrowedBooksByUserAsync(string userId)
        {
            var borrowedRecords = await _context.BorrowRecords
                .Where(br => br.UserId == userId)
                .Include(br => br.Book)
                .OrderByDescending(br => br.BorrowDate)
                .Select(br => new BorrowRecordDto
                {
                    Id = br.Id,
                    BookId = br.BookId,
                    BookTitle = br.Book.Title,
                    BookAuthor = br.Book.Author,
                    UserId = br.UserId,
                    BorrowDate = br.BorrowDate,
                    DueDate = br.DueDate,
                    ReturnDate = br.ReturnDate,
                    Status = br.Status.ToString(),
                    Notes = br.Notes
                })
                .ToListAsync();

            return borrowedRecords;
        }

        public async Task<ReturnBookResponseDto> ReturnBookAsync(int borrowRecordId)
        {
            var borrowRecord = await _context.BorrowRecords
                .Include(br => br.Book)
                .FirstOrDefaultAsync(br => br.Id == borrowRecordId);

            if (borrowRecord == null)
            {
                throw new ArgumentException("Borrow record not found.");
            }

            if (borrowRecord.Status != BorrowStatus.Borrowed)
            {
                throw new InvalidOperationException("This book is not currently borrowed.");
            }

            var returnDate = DateTime.UtcNow;
            var isOverdue = returnDate > borrowRecord.DueDate;
            decimal? fineAmount = null;

            // Calculate fine if overdue (e.g., $1 per day)
            if (isOverdue)
            {
                var overdueDays = (returnDate - borrowRecord.DueDate).Days;
                fineAmount = overdueDays * 1.0m; // $1 per day fine

                // Create a fine record
                var fine = new Fine
                {
                    UserId = borrowRecord.UserId,
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
                Message = isOverdue ? $"Book returned successfully. Fine: ${fineAmount:F2}" : "Book returned successfully."
            };
        }
    }
}
