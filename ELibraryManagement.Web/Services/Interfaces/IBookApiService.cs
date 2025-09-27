using ELibraryManagement.Web.Models;
using ELibraryManagement.Web.Models.ViewModels;

namespace ELibraryManagement.Web.Services.Interfaces
{
    public interface IBookApiService
    {
        Task<List<BookViewModel>> GetAvailableBooksAsync();
        Task<PagedResult<BookViewModel>> GetAvailableBooksPagedAsync(string? search, string? category, string? author, string? sortBy, int page = 1, int pageSize = 12);
        Task<List<BookViewModel>> GetAvailableBooksAsync(string? search, string? category, string? author, string? sortBy, int page = 1, int pageSize = 12);
        Task<BookViewModel?> GetBookByIdAsync(int id);
        Task<List<BookViewModel>> GetRelatedBooksAsync(int excludeId, string? categoryName, int count = 4);
        Task<List<string>> GetCategoriesAsync();
        Task<List<string>> GetAuthorsAsync();
        Task<BorrowBookResponseViewModel> BorrowBookAsync(BorrowBookRequestViewModel request, string token);
        Task<List<UserBorrowedBookViewModel>> GetBorrowedBooksAsync(string userId, string token);
        Task<List<UserBorrowedBookViewModel>> GetBorrowHistoryAsync(string userId, string token);
        Task<bool> HasUserBorrowedBookAsync(string userId, int bookId, string token);
        Task<BorrowBookResponseViewModel> ReturnBookAsync(int borrowRecordId, string token);
        Task<BorrowBookResponseViewModel> CancelBorrowRequestAsync(int borrowRecordId, string token);
    }
}