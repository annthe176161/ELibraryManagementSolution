using ELibraryManagement.Api.DTOs;

namespace ELibraryManagement.Api.Services.Interfaces
{
    public interface IBorrowService
    {
        Task<IEnumerable<BorrowRecordDto>> GetAllBorrowRecordsAsync();
        Task<BorrowRecordDto?> GetBorrowRecordByIdAsync(int id);
        Task<bool> UpdateBorrowStatusAsync(int id, UpdateBorrowStatusDto updateDto);
        Task<bool> UpdateBorrowNotesAsync(int id, string? notes);
        Task<ReturnBookResponseDto> ConfirmReturnAsync(int id);
        Task<IEnumerable<BorrowRecordDto>> GetOverdueBorrowsAsync();
        Task<IEnumerable<BorrowRecordDto>> GetBorrowsByStatusAsync(string status);
    }
}