using ELibraryManagement.Api.Models;

namespace ELibraryManagement.Api.Services.Interfaces
{
    public interface IBorrowStatusValidationService
    {
        /// <summary>
        /// Kiểm tra xem có thể chuyển từ trạng thái hiện tại sang trạng thái mới không
        /// </summary>
        bool CanTransition(BorrowStatus currentStatus, BorrowStatus newStatus);

        /// <summary>
        /// Lấy danh sách các trạng thái có thể chuyển từ trạng thái hiện tại
        /// </summary>
        IEnumerable<BorrowStatus> GetAllowedTransitions(BorrowStatus currentStatus);

        /// <summary>
        /// Kiểm tra xem trạng thái có phải là trạng thái cuối không (không thể chuyển tiếp)
        /// </summary>
        bool IsFinalStatus(BorrowStatus status);

        /// <summary>
        /// Lấy thông báo lỗi khi không thể chuyển trạng thái
        /// </summary>
        string GetTransitionErrorMessage(BorrowStatus currentStatus, BorrowStatus newStatus);
    }
}