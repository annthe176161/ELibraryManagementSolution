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
        private readonly IUserStatusService _userStatusService;

        public BookService(ApplicationDbContext context, IUserStatusService userStatusService)
        {
            _context = context;
            _userStatusService = userStatusService;
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
            // Check if user can borrow
            var canBorrow = await _userStatusService.CanUserBorrowAsync(request.UserId);
            if (!canBorrow)
            {
                var userStatus = await _userStatusService.GetUserStatusAsync(request.UserId);
                if (userStatus.AccountStatus == UserAccountStatus.Blocked)
                {
                    throw new InvalidOperationException($"Tài khoản của bạn đã bị khóa. Lý do: {userStatus.BlockReason}");
                }
                if (userStatus.CurrentBorrowCount >= userStatus.MaxBorrowLimit)
                {
                    throw new InvalidOperationException($"Bạn đã đạt giới hạn mượn sách ({userStatus.MaxBorrowLimit} cuốn).");
                }
                if (userStatus.TotalOutstandingFines > 50000)
                {
                    throw new InvalidOperationException($"Bạn có khoản phạt chưa thanh toán là {userStatus.TotalOutstandingFines:N0} VND. Vui lòng thanh toán phạt trước khi mượn sách.");
                }
            }

            // Check if book exists and is available
            var book = await _context.Books.FindAsync(request.BookId);
            if (book == null)
            {
                throw new ArgumentException("Không tìm thấy sách.");
            }

            if (book.AvailableQuantity <= 0)
            {
                throw new InvalidOperationException("Sách không có sẵn để mượn.");
            }

            // Check if user already has this book borrowed
            var existingBorrow = await _context.BorrowRecords
                .AnyAsync(br => br.UserId == request.UserId && br.BookId == request.BookId && br.Status == BorrowStatus.Borrowed);

            if (existingBorrow)
            {
                throw new InvalidOperationException("Bạn đã mượn cuốn sách này rồi.");
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

            // Update user status - increment borrow count
            await _userStatusService.IncrementBorrowCountAsync(request.UserId);

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
                Message = "Mượn sách thành công."
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
                    BookCoverUrl = br.Book.CoverImageUrl,
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
                throw new ArgumentException("Không tìm thấy bản ghi mượn sách.");
            }

            if (borrowRecord.Status != BorrowStatus.Borrowed)
            {
                throw new InvalidOperationException("Cuốn sách này hiện không được mượn.");
            }

            var returnDate = DateTime.UtcNow;
            var isOverdue = returnDate > borrowRecord.DueDate;
            decimal? fineAmount = null;

            // Calculate fine if overdue using VND rates
            if (isOverdue)
            {
                var overdueDays = (returnDate - borrowRecord.DueDate).Days;

                // Progressive fine rates as per business rules
                decimal dailyRate = 0;
                if (overdueDays <= 7)
                    dailyRate = 2000; // 2,000 VND per day for first week
                else if (overdueDays <= 14)
                    dailyRate = 5000; // 5,000 VND per day for second week
                else
                    dailyRate = 10000; // 10,000 VND per day after 2 weeks

                fineAmount = overdueDays * dailyRate;

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

                // Add fine to user status
                await _userStatusService.AddFineAsync(borrowRecord.UserId, fineAmount.Value);
            }

            // Update borrow record
            borrowRecord.ReturnDate = returnDate;
            borrowRecord.Status = BorrowStatus.Returned;
            borrowRecord.UpdatedAt = DateTime.UtcNow;

            // Increase available quantity
            borrowRecord.Book.AvailableQuantity++;

            // Decrease user's current borrow count
            await _userStatusService.DecrementBorrowCountAsync(borrowRecord.UserId);

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
                Message = isOverdue ? $"Trả sách thành công. Phạt: {fineAmount:N0} VND" : "Trả sách thành công."
            };
        }

        // Admin functions
        public async Task<BookDto> CreateBookAsync(CreateBookDto createBookDto)
        {
            // Debug logging
            Console.WriteLine($"[BookService] CreateBookAsync - Input CoverImageUrl: '{createBookDto.CoverImageUrl}'");

            // Validate ISBN length
            if (!string.IsNullOrEmpty(createBookDto.ISBN) && createBookDto.ISBN.Length > 13)
            {
                throw new ArgumentException("ISBN không được vượt quá 13 ký tự");
            }

            var book = new Book
            {
                Title = createBookDto.Title,
                Author = createBookDto.Author,
                ISBN = createBookDto.ISBN,
                Publisher = createBookDto.Publisher,
                PublicationYear = createBookDto.PublicationYear,
                Description = createBookDto.Description,
                CoverImageUrl = createBookDto.CoverImageUrl,
                Quantity = createBookDto.Quantity,
                AvailableQuantity = createBookDto.Quantity, // Initially all books are available
                Language = createBookDto.Language,
                PageCount = createBookDto.PageCount,
                AverageRating = 0,
                RatingCount = 0,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            Console.WriteLine($"[BookService] CreateBookAsync - Book object CoverImageUrl before save: '{book.CoverImageUrl}'");

            try
            {
                _context.Books.Add(book);
                await _context.SaveChangesAsync();

                Console.WriteLine($"[BookService] CreateBookAsync - Book ID after save: {book.Id}, CoverImageUrl: '{book.CoverImageUrl}'");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BookService] CreateBookAsync - SaveChanges Exception: {ex.Message}");
                Console.WriteLine($"[BookService] CreateBookAsync - Inner Exception: {ex.InnerException?.Message}");
                Console.WriteLine($"[BookService] CreateBookAsync - Stack Trace: {ex.StackTrace}");
                throw new Exception($"Database save failed: {ex.InnerException?.Message ?? ex.Message}", ex);
            }

            // Add categories if provided
            if (createBookDto.CategoryIds?.Any() == true)
            {
                var bookCategories = createBookDto.CategoryIds.Select(categoryId => new BookCategory
                {
                    BookId = book.Id,
                    CategoryId = categoryId
                }).ToList();

                _context.BookCategories.AddRange(bookCategories);
                await _context.SaveChangesAsync();
            }

            // Return the created book with categories
            return await GetBookByIdAsync(book.Id) ?? throw new InvalidOperationException("Không thể lấy thông tin sách đã tạo");
        }

        public async Task<BookDto> UpdateBookAsync(UpdateBookDto updateBookDto)
        {
            var book = await _context.Books
                .Include(b => b.BookCategories)
                .FirstOrDefaultAsync(b => b.Id == updateBookDto.Id && !b.IsDeleted);

            if (book == null)
            {
                throw new ArgumentException($"Không tìm thấy sách với ID {updateBookDto.Id}");
            }

            // Update book properties
            book.Title = updateBookDto.Title;
            book.Author = updateBookDto.Author;
            book.ISBN = updateBookDto.ISBN;
            book.Publisher = updateBookDto.Publisher;
            book.PublicationYear = updateBookDto.PublicationYear;
            book.Description = updateBookDto.Description;
            book.CoverImageUrl = updateBookDto.CoverImageUrl;

            // Update quantity but maintain available quantity ratio
            var borrowedQuantity = book.Quantity - book.AvailableQuantity;
            book.Quantity = updateBookDto.Quantity;
            book.AvailableQuantity = Math.Max(0, updateBookDto.Quantity - borrowedQuantity);

            book.Language = updateBookDto.Language;
            book.PageCount = updateBookDto.PageCount;
            book.UpdatedAt = DateTime.UtcNow;

            // Update categories
            if (updateBookDto.CategoryIds != null)
            {
                // Remove existing categories
                _context.BookCategories.RemoveRange(book.BookCategories);

                // Add new categories
                var newBookCategories = updateBookDto.CategoryIds.Select(categoryId => new BookCategory
                {
                    BookId = book.Id,
                    CategoryId = categoryId
                }).ToList();

                _context.BookCategories.AddRange(newBookCategories);
            }

            await _context.SaveChangesAsync();

            // Return the updated book with categories
            return await GetBookByIdAsync(book.Id) ?? throw new InvalidOperationException("Không thể lấy thông tin sách đã cập nhật");
        }

        public async Task<bool> DeleteBookAsync(int id)
        {
            var book = await _context.Books
                .Include(b => b.BorrowRecords)
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);

            if (book == null)
            {
                return false;
            }

            // Check if book has active borrows
            var hasActiveBorrows = book.BorrowRecords.Any(br => br.Status == BorrowStatus.Borrowed);
            if (hasActiveBorrows)
            {
                throw new InvalidOperationException("Không thể xóa sách đang được mượn");
            }

            // Soft delete
            book.IsDeleted = true;
            book.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<BookDto>> GetAllBooksAsync()
        {
            return await _context.Books
                .Where(b => !b.IsDeleted)
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
                }).ToListAsync();
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .Where(c => !c.IsDeleted)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Color = c.Color
                }).ToListAsync();
        }
    }
}
