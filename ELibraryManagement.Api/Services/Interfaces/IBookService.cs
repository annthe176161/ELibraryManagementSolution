using ELibraryManagement.Api.DTOs;

namespace ELibraryManagement.Api.Services.Interfaces
{
    public interface IBookService
    {
        IQueryable<BookDto> GetAvailableBooksQueryable();
        Task<BookDto?> GetBookByIdAsync(int id);
        Task<BorrowBookResponseDto> BorrowBookAsync(BorrowBookRequestDto request);
        Task<IEnumerable<BorrowRecordDto>> GetBorrowedBooksByUserAsync(string userId);
        Task<ReturnBookResponseDto> ReturnBookAsync(int borrowRecordId);
    }
}
