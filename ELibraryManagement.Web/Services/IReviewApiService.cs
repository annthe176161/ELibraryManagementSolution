using ELibraryManagement.Web.Models;

namespace ELibraryManagement.Web.Services
{
    public interface IReviewApiService
    {
        // Tạo review mới
        Task<ReviewResponseViewModel> CreateReviewAsync(CreateReviewViewModel createReview, string token);

        // Cập nhật review
        Task<ReviewResponseViewModel> UpdateReviewAsync(UpdateReviewViewModel updateReview, string token);

        // Xóa review
        Task<ReviewResponseViewModel> DeleteReviewAsync(int reviewId, string token);

        // Lấy review theo ID
        Task<ReviewViewModel?> GetReviewByIdAsync(int reviewId);

        // Lấy reviews của một sách với phân trang
        Task<ReviewListViewModel> GetBookReviewsAsync(int bookId, int page = 1, int pageSize = 10);

        // Lấy tổng quan review của một sách
        Task<BookReviewSummaryViewModel> GetBookReviewSummaryAsync(int bookId);

        // Lấy reviews của user hiện tại
        Task<List<ReviewViewModel>> GetMyReviewsAsync(string token);

        // Kiểm tra có thể review sách không
        Task<CanReviewViewModel> CanReviewBookAsync(int bookId, string token);

        // Lấy review của user cho một sách
        Task<ReviewViewModel?> GetMyReviewForBookAsync(int bookId, string token);
    }
}