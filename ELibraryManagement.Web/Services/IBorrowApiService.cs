using ELibraryManagement.Web.Models;

namespace ELibraryManagement.Web.Services
{
    public interface IBorrowApiService
    {
        Task<List<BorrowRecordViewModel>?> GetAllBorrowRecordsAsync();
        Task<BorrowRecordViewModel?> GetBorrowRecordByIdAsync(int id);
        Task<BorrowDetailViewModel?> GetBorrowDetailAsync(int borrowId);
        Task<bool> UpdateBorrowStatusAsync(int borrowId, string status, string? notes);
        Task<bool> ApproveBorrowRequestAsync(int borrowId);
        Task<ReturnBookResponseViewModel?> ConfirmReturnAsync(int borrowId);
        Task<List<BorrowRecordViewModel>?> GetOverdueBorrowsAsync();
        Task<bool> SendReminderAsync(int borrowId);
        Task<BorrowResult> BorrowBookAsync(int bookId);
        Task<bool> CancelBorrowRequestAsync(int borrowId);
        Task<List<BorrowRecordViewModel>?> GetMyBorrowsAsync();
        Task<bool> IsAuthenticatedAsync();
        void SetAuthToken(string token);
    }
}
