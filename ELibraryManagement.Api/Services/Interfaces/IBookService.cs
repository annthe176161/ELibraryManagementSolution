using ELibraryManagement.Api.DTOs;

namespace ELibraryManagement.Api.Services.Interfaces
{
    public interface IBookService
    {
        IQueryable<BookDto> GetAvailableBooksQueryable();
        Task<BookDto?> GetBookByIdAsync(int id);
        Task<BorrowBookResponseDto> BorrowBookAsync(BorrowBookRequestDto request);
        Task<IEnumerable<BorrowRecordDto>> GetBorrowedBooksByUserAsync(string userId);
        Task<IEnumerable<BorrowRecordDto>> GetBorrowHistoryByUserAsync(string userId);
        Task<ReturnBookResponseDto> ReturnBookAsync(int borrowRecordId);
        Task<CancelBorrowRequestResponseDto> CancelBorrowRequestAsync(int borrowRecordId);

        // Admin functions
        Task<BookDto> CreateBookAsync(CreateBookDto createBookDto);
        Task<BookDto> UpdateBookAsync(UpdateBookDto updateBookDto);
        Task<bool> DeleteBookAsync(int id);
        Task<IEnumerable<BookDto>> GetAllBooksAsync();
        Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
    }
}
