using System.ComponentModel.DataAnnotations;

namespace ELibraryManagement.Api.DTOs
{
    // DTO cho tạo review mới
    public class CreateReviewDto
    {
        [Required]
        public int BookId { get; set; }

        [Range(1, 5, ErrorMessage = "Rating phải từ 1 đến 5")]
        public int Rating { get; set; }

        [MaxLength(1000, ErrorMessage = "Comment không được quá 1000 ký tự")]
        public string? Comment { get; set; }

        public int? BorrowRecordId { get; set; } // Liên kết với borrow record để kiểm tra điều kiện
    }

    // DTO cho cập nhật review
    public class UpdateReviewDto
    {
        [Range(1, 5, ErrorMessage = "Rating phải từ 1 đến 5")]
        public int Rating { get; set; }

        [MaxLength(1000, ErrorMessage = "Comment không được quá 1000 ký tự")]
        public string? Comment { get; set; }
    }

    // DTO cho hiển thị review
    public class ReviewDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string UserAvatarUrl { get; set; } = string.Empty;
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string BookAuthor { get; set; } = string.Empty;
        public string BookCoverUrl { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime ReviewDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool CanEdit { get; set; } // Cho phép edit nếu là chủ review
    }

    // DTO cho thống kê review của sách
    public class BookReviewSummaryDto
    {
        public int BookId { get; set; }
        public int TotalReviews { get; set; }
        public double AverageRating { get; set; }
        public int FiveStarCount { get; set; }
        public int FourStarCount { get; set; }
        public int ThreeStarCount { get; set; }
        public int TwoStarCount { get; set; }
        public int OneStarCount { get; set; }
        public List<ReviewDto> RecentReviews { get; set; } = new();
    }

    // DTO cho response sau khi tạo/cập nhật review
    public class ReviewResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public ReviewDto? Review { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}