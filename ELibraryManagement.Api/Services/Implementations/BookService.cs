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
                    Price = b.Price,
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
                Price = book.Price,
                Language = book.Language,
                PageCount = book.PageCount,
                AverageRating = (float)book.AverageRating,
                RatingCount = book.RatingCount,
                Categories = book.BookCategories?.Select(bc => new CategoryDto
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
            // Validate book exists and is available
            var book = await _context.Books
                .FirstOrDefaultAsync(b => b.Id == request.BookId && !b.IsDeleted);

            if (book == null)
            {
                return new BorrowBookResponseDto
                {
                    Message = "Book not found."
                };
            }

            if (book.AvailableQuantity <= 0)
            {
                return new BorrowBookResponseDto
                {
                    Message = "Book is not available for borrowing."
                };
            }

            // Validate user exists
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == request.UserId);

            if (user == null)
            {
                return new BorrowBookResponseDto
                {
                    Message = "User not found."
                };
            }

            // Check if user already has this book borrowed and not returned
            var existingBorrow = await _context.BorrowRecords
                .FirstOrDefaultAsync(br => br.UserId == request.UserId &&
                                         br.BookId == request.BookId &&
                                         br.Status == BorrowStatus.Borrowed);

            if (existingBorrow != null)
            {
                return new BorrowBookResponseDto
                {
                    Message = "You have already borrowed this book and haven't returned it yet."
                };
            }

            // Set default due date if not provided (14 days from now)
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
                RentalPrice = book.Price, // Use book's price as rental price
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
                RentalPrice = borrowRecord.RentalPrice,
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
                    RentalPrice = br.RentalPrice,
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
                return new ReturnBookResponseDto
                {
                    Success = false,
                    Message = "Borrow record not found."
                };
            }

            if (borrowRecord.Status == BorrowStatus.Returned)
            {
                return new ReturnBookResponseDto
                {
                    Success = false,
                    Message = "Book has already been returned."
                };
            }

            // Calculate fine if overdue
            decimal? fineAmount = null;
            if (DateTime.UtcNow > borrowRecord.DueDate)
            {
                var overdueDays = (DateTime.UtcNow - borrowRecord.DueDate).Days;
                fineAmount = overdueDays * 1000; // 1000 VND per day overdue
            }

            // Update borrow record
            borrowRecord.ReturnDate = DateTime.UtcNow;
            borrowRecord.Status = BorrowStatus.Returned;
            borrowRecord.UpdatedAt = DateTime.UtcNow;

            // Increase available quantity
            borrowRecord.Book.AvailableQuantity++;

            // Create fine record if overdue
            if (fineAmount > 0)
            {
                var fine = new Fine
                {
                    BorrowRecordId = borrowRecord.Id,
                    Amount = fineAmount.Value,
                    Reason = $"Overdue return: {borrowRecord.Book.Title}",
                    Status = FineStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Fines.Add(fine);
            }

            await _context.SaveChangesAsync();

            return new ReturnBookResponseDto
            {
                Success = true,
                BorrowRecordId = borrowRecord.Id,
                BookId = borrowRecord.BookId,
                BookTitle = borrowRecord.Book.Title,
                UserId = borrowRecord.UserId,
                ReturnDate = borrowRecord.ReturnDate.Value,
                FineAmount = fineAmount,
                Message = fineAmount > 0 ? $"Book returned successfully. Fine amount: {fineAmount} VND" : "Book returned successfully."
            };
        }
    }
}
