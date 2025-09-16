using ELibraryManagement.Api.DTOs;

namespace ELibraryManagement.Api.Services.Interfaces
{
    public interface IBorrowService
    {
        Task<IEnumerable<BorrowRecordDto>> GetAllBorrowRecordsAsync();
        Task<BorrowRecordDto?> GetBorrowRecordByIdAsync(int id);
        Task<bool> UpdateBorrowStatusAsync(int id, UpdateBorrowStatusDto updateDto);
        Task<bool> UpdateBorrowNotesAsync(int id, string? notes);
        Task<bool> ExtendDueDateAsync(int id, DateTime newDueDate);
        Task<ExtendBorrowResponseDto> ExtendBorrowAsync(int id, string? reason = null);
        Task<bool> SendReminderAsync(int id);
        Task<ReturnBookResponseDto> ConfirmReturnAsync(int id);
        Task<IEnumerable<BorrowRecordDto>> GetOverdueBorrowsAsync();
        Task<IEnumerable<BorrowRecordDto>> GetBorrowsByStatusAsync(string status);
    }
}