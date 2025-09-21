using ELibraryManagement.Api.Data;
using ELibraryManagement.Api.DTOs;
using ELibraryManagement.Api.Models;
using ELibraryManagement.Api.Services.Interfaces;
using ELibraryManagement.Api.Helpers;
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
            // Use the simpler approach - rely on AvailableQuantity field in database
            // This field should be kept in sync by the borrow/return operations
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

            // Calculate actual available quantity based on borrowed books only
            // Treat Overdue and Lost as borrowed so overdue and lost copies are not counted as available
            var borrowedCount = await _context.BorrowRecords
                .CountAsync(br => br.BookId == book.Id && (br.Status == BorrowStatus.Borrowed || br.Status == BorrowStatus.Overdue || br.Status == BorrowStatus.Lost));

            // Calculate requested count for statistics
            var requestedCount = await _context.BorrowRecords
                .CountAsync(br => br.BookId == book.Id && br.Status == BorrowStatus.Requested);

            var actualAvailableQuantity = book.Quantity - borrowedCount;

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
                AvailableQuantity = Math.Max(0, actualAvailableQuantity), // Ensure not negative
                RequestedCount = requestedCount, // Add requested count for admin view
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

            // Check if user already has this book borrowed or requested
            var existingBorrow = await _context.BorrowRecords
                .AnyAsync(br => br.UserId == request.UserId && br.BookId == request.BookId &&
                         (br.Status == BorrowStatus.Borrowed ||
                          br.Status == BorrowStatus.Requested ||
                          br.Status == BorrowStatus.Overdue));

            if (existingBorrow)
            {
                // Get the specific borrow record to provide more detailed information
                var borrowRecord = await _context.BorrowRecords
                    .Where(br => br.UserId == request.UserId && br.BookId == request.BookId &&
                           (br.Status == BorrowStatus.Borrowed ||
                            br.Status == BorrowStatus.Requested ||
                            br.Status == BorrowStatus.Overdue))
                    .OrderByDescending(br => br.CreatedAt)
                    .FirstOrDefaultAsync();

                if (borrowRecord != null)
                {
                    string statusMessage = "";
                    switch (borrowRecord.Status)
                    {
                        case BorrowStatus.Requested:
                            statusMessage = "Bạn đã có yêu cầu mượn cuốn sách này và đang chờ xác nhận từ thủ thư.";
                            break;
                        case BorrowStatus.Borrowed:
                            statusMessage = $"Bạn đang mượn cuốn sách này. Hạn trả: {borrowRecord.DueDate:dd/MM/yyyy}.";
                            break;
                        case BorrowStatus.Overdue:
                            statusMessage = $"Bạn đang mượn cuốn sách này nhưng đã quá hạn. Hạn trả: {borrowRecord.DueDate:dd/MM/yyyy}. Vui lòng trả sách trước khi mượn sách khác.";
                            break;
                        default:
                            statusMessage = "Bạn đã có liên quan đến cuốn sách này.";
                            break;
                    }
                    throw new InvalidOperationException(statusMessage);
                }
                else
                {
                    throw new InvalidOperationException("Bạn đã có yêu cầu mượn hoặc đang mượn cuốn sách này rồi.");
                }
            }

            // Calculate due date (default 14 days if not provided)
            var dueDate = request.DueDate ?? DateTime.UtcNow.AddDays(14);

            // Create borrow record with Requested status (pending admin approval)
            var newBorrowRecord = new BorrowRecord
            {
                UserId = request.UserId,
                BookId = request.BookId,
                BorrowDate = DateTime.UtcNow,
                DueDate = dueDate,
                Status = BorrowStatus.Requested,
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow
            };

            // Note: Don't decrease available quantity for Requested status
            // Only decrease when status changes to Borrowed (when admin approves)

            // NOTE: Do NOT increment stored CurrentBorrowCount for Requested status here.
            // The CurrentBorrowCount represents actual 'Borrowed' items. Incrementing should happen
            // when an admin confirms and the status becomes Borrowed. This avoids blocking users
            // who have many requests but fewer active borrows.

            // Save changes
            _context.BorrowRecords.Add(newBorrowRecord);
            await _context.SaveChangesAsync();

            return new BorrowBookResponseDto
            {
                BorrowRecordId = newBorrowRecord.Id,
                BookId = book.Id,
                BookTitle = book.Title,
                UserId = request.UserId,
                BorrowDate = newBorrowRecord.BorrowDate,
                DueDate = newBorrowRecord.DueDate,
                Status = newBorrowRecord.Status.ToString(),
                Message = "Yêu cầu mượn sách đã được gửi và đang chờ xác nhận từ thủ thư."
            };
        }

        public async Task<IEnumerable<BorrowRecordDto>> GetBorrowedBooksByUserAsync(string userId)
        {
            // Use int values for enum comparison to ensure proper SQL translation
            var requestedStatus = (int)BorrowStatus.Requested;
            var borrowedStatus = (int)BorrowStatus.Borrowed;
            var overdueStatus = (int)BorrowStatus.Overdue;

            var borrowedRecords = await _context.BorrowRecords
                .Where(br => br.UserId == userId && ((int)br.Status == requestedStatus || (int)br.Status == borrowedStatus || (int)br.Status == overdueStatus))
                .Include(br => br.Book)
                .OrderByDescending(br => br.BorrowDate)
                .Select(br => new BorrowRecordDto
                {
                    Id = br.Id,
                    BookId = br.BookId,
                    BookTitle = br.Book.Title,
                    BookAuthor = br.Book.Author,
                    BookCoverUrl = br.Book.CoverImageUrl ?? "",
                    UserId = br.UserId,
                    ConfirmedDate = br.ConfirmedDate,
                    BorrowDate = br.BorrowDate,
                    DueDate = br.DueDate,
                    ReturnDate = br.ReturnDate,
                    Status = br.Status.ToString(),
                    Notes = br.Notes
                })
                .ToListAsync();

            // Debug logging
            Console.WriteLine($"Found {borrowedRecords.Count} records for user {userId} using int comparison");
            foreach (var record in borrowedRecords)
            {
                Console.WriteLine($"Book: {record.BookTitle}, Status: {record.Status}");
            }

            return borrowedRecords;
        }

        public async Task<IEnumerable<BorrowRecordDto>> GetBorrowHistoryByUserAsync(string userId)
        {
            var borrowHistory = await _context.BorrowRecords
                .Where(br => br.UserId == userId)
                .Include(br => br.Book)
                .Include(br => br.Fines) // Include Fine information
                .OrderByDescending(br => br.BorrowDate)
                .Select(br => new BorrowRecordDto
                {
                    Id = br.Id,
                    BookId = br.BookId,
                    BookTitle = br.Book.Title,
                    BookAuthor = br.Book.Author,
                    BookCoverUrl = br.Book.CoverImageUrl ?? "",
                    UserId = br.UserId,
                    ConfirmedDate = br.ConfirmedDate,
                    BorrowDate = br.BorrowDate,
                    DueDate = br.DueDate,
                    ReturnDate = br.ReturnDate,
                    Status = br.Status.ToString(),
                    Notes = br.Notes,
                    // Add fine information
                    FineAmount = br.Fines.Where(f => f.BorrowRecordId == br.Id).Sum(f => f.Amount),
                    FineStatus = br.Fines.Any(f => f.BorrowRecordId == br.Id) ?
                                br.Fines.Where(f => f.BorrowRecordId == br.Id).OrderByDescending(f => f.CreatedAt).First().Status.ToString() : null,
                    FineReason = br.Fines.Any(f => f.BorrowRecordId == br.Id) ?
                                br.Fines.Where(f => f.BorrowRecordId == br.Id).OrderByDescending(f => f.CreatedAt).First().Reason : null
                })
                .ToListAsync();

            // Debug logging
            Console.WriteLine($"Found {borrowHistory.Count} history records for user {userId}");
            foreach (var record in borrowHistory)
            {
                Console.WriteLine($"Book: {record.BookTitle}, Status: {record.Status}, Fine: {record.FineAmount}");
            }

            return borrowHistory;
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

                // Fixed fine rate: 5000 VND per day
                var finePerDay = 5000m; // 5000 VND per day
                fineAmount = overdueDays * finePerDay;

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

        public async Task<CancelBorrowRequestResponseDto> CancelBorrowRequestAsync(int borrowRecordId)
        {
            var borrowRecord = await _context.BorrowRecords
                .Include(br => br.Book)
                .Include(br => br.User)
                .FirstOrDefaultAsync(br => br.Id == borrowRecordId);

            if (borrowRecord == null)
            {
                throw new ArgumentException("Không tìm thấy bản ghi mượn sách.");
            }

            // Only allow cancellation if status is Requested
            if (borrowRecord.Status != Models.BorrowStatus.Requested)
            {
                throw new InvalidOperationException("Chỉ có thể hủy yêu cầu mượn sách khi trạng thái là 'Chờ xác nhận'.");
            }

            // Update status to Cancelled
            borrowRecord.Status = Models.BorrowStatus.Cancelled;
            borrowRecord.UpdatedAt = DateTime.UtcNow;
            // Note: Don't set ReturnDate for cancelled requests as they were never actually borrowed

            // Don't increase book available quantity because we never decreased it when the request was created
            // Only decrease available quantity when admin approves the request (status changes to Borrowed)

            // Previously this code decremented the user's CurrentBorrowCount because
            // requests were counted when created. After the change to not count
            // requests at creation time, there's nothing to decrement here.
            // If in the future requests are counted again, adjust this logic accordingly.

            await _context.SaveChangesAsync();

            return new CancelBorrowRequestResponseDto
            {
                Success = true,
                BorrowRecordId = borrowRecord.Id,
                BookId = borrowRecord.BookId,
                BookTitle = borrowRecord.Book.Title,
                UserId = borrowRecord.UserId,
                CancelDate = DateTime.UtcNow,
                Message = "Hủy yêu cầu mượn sách thành công."
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
            var books = await _context.Books
                .Where(b => !b.IsDeleted)
                .Include(b => b.BookCategories)
                .ThenInclude(bc => bc.Category)
                .ToListAsync();

            var bookDtos = new List<BookDto>();

            foreach (var book in books)
            {
                // Calculate actual available quantity based on borrowed books only
                // Treat Overdue and Lost as borrowed so overdue and lost copies are not counted as available
                var borrowedCount = await _context.BorrowRecords
                    .CountAsync(br => br.BookId == book.Id && (br.Status == BorrowStatus.Borrowed || br.Status == BorrowStatus.Overdue || br.Status == BorrowStatus.Lost));

                // Calculate requested count for statistics
                var requestedCount = await _context.BorrowRecords
                    .CountAsync(br => br.BookId == book.Id && br.Status == BorrowStatus.Requested);

                var actualAvailableQuantity = book.Quantity - borrowedCount;

                bookDtos.Add(new BookDto
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
                    AvailableQuantity = Math.Max(0, actualAvailableQuantity), // Ensure not negative
                    RequestedCount = requestedCount, // Add requested count for admin view
                    Language = book.Language,
                    PageCount = book.PageCount,
                    AverageRating = (float)book.AverageRating,
                    RatingCount = book.RatingCount,
                    Categories = book.BookCategories.Select(bc => new CategoryDto
                    {
                        Id = bc.Category.Id,
                        Name = bc.Category.Name,
                        Description = bc.Category.Description,
                        Color = bc.Category.Color,
                        IsActive = bc.Category.IsActive,
                        CreatedAt = bc.Category.CreatedAt,
                        UpdatedAt = bc.Category.UpdatedAt,
                        BookCount = 0 // Set to 0 for performance, not needed in book context
                    }).ToList()
                });
            }

            return bookDtos;
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

        public async Task<int> SyncAvailableQuantitiesAsync()
        {
            var books = await _context.Books.Where(b => !b.IsDeleted).ToListAsync();
            int updatedCount = 0;

            foreach (var book in books)
            {
                // Calculate actual borrowed count
                // Treat Overdue and Lost as borrowed so sync keeps AvailableQuantity accurate
                var borrowedCount = await _context.BorrowRecords
                    .CountAsync(br => br.BookId == book.Id && (br.Status == BorrowStatus.Borrowed || br.Status == BorrowStatus.Overdue || br.Status == BorrowStatus.Lost));

                var correctAvailableQuantity = Math.Max(0, book.Quantity - borrowedCount);

                if (book.AvailableQuantity != correctAvailableQuantity)
                {
                    book.AvailableQuantity = correctAvailableQuantity;
                    updatedCount++;
                }
            }

            if (updatedCount > 0)
            {
                await _context.SaveChangesAsync();
            }

            return updatedCount;
        }

        public async Task<bool> DecrementAvailableQuantityAsync(int bookId)
        {
            try
            {
                var book = await _context.Books.FindAsync(bookId);
                if (book == null || book.AvailableQuantity <= 0)
                {
                    return false;
                }

                book.AvailableQuantity--;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
