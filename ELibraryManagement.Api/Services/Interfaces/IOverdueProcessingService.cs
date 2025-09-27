using ELibraryManagement.Api.Models;

namespace ELibraryManagement.Api.Services
{
    public interface IOverdueProcessingService
    {
        /// <summary>
        /// Xử lý tất cả sách quá hạn: cập nhật trạng thái và tạo phạt
        /// </summary>
        /// <returns>Số lượng borrow record đã được xử lý</returns>
        Task<int> ProcessOverdueBooksAsync();

        /// <summary>
        /// Kiểm tra và cập nhật trạng thái một borrow record cụ thể
        /// </summary>
        /// <param name="borrowRecordId">ID của borrow record</param>
        /// <returns>True nếu cần cập nhật, False nếu không</returns>
        Task<bool> ProcessSingleBorrowRecordAsync(int borrowRecordId);

        /// <summary>
        /// Tính toán số tiền phạt dựa trên số ngày quá hạn
        /// </summary>
        /// <param name="overdueDays">Số ngày quá hạn</param>
        /// <returns>Số tiền phạt</returns>
        decimal CalculateFineAmount(int overdueDays);
    }
}