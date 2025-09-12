using ELibraryManagement.Api.DTOs;

namespace ELibraryManagement.Api.Services.Interfaces
{
    public interface IReviewService
    {
        // Tạo review mới
        Task<ReviewResponseDto> CreateReviewAsync(string userId, CreateReviewDto createReviewDto);

        // Cập nhật review
        Task<ReviewResponseDto> UpdateReviewAsync(string userId, int reviewId, UpdateReviewDto updateReviewDto);

        // Xóa review
        Task<ReviewResponseDto> DeleteReviewAsync(string userId, int reviewId);

        // Lấy review theo ID
        Task<ReviewDto?> GetReviewByIdAsync(int reviewId);

        // Lấy reviews của một sách với phân trang
        Task<(List<ReviewDto> Reviews, int TotalCount)> GetBookReviewsAsync(int bookId, int page = 1, int pageSize = 10);

        // Lấy review summary của một sách
        Task<BookReviewSummaryDto> GetBookReviewSummaryAsync(int bookId);

        // Lấy reviews của một user
        Task<List<ReviewDto>> GetUserReviewsAsync(string userId);

        // Kiểm tra user có thể review sách này không (đã từng mượn và trả)
        Task<bool> CanUserReviewBookAsync(string userId, int bookId);

        // Kiểm tra user đã review sách này chưa
        Task<ReviewDto?> GetUserReviewForBookAsync(string userId, int bookId);
    }
}