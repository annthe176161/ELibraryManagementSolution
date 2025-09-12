using System.ComponentModel.DataAnnotations;

namespace ELibraryManagement.Web.Models
{
    // ViewModel cho tạo review mới
    public class CreateReviewViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn sách")]
        public int BookId { get; set; }

        public string BookTitle { get; set; } = string.Empty;
        public string BookAuthor { get; set; } = string.Empty;
        public string BookCoverUrl { get; set; } = string.Empty;

        [Range(1, 5, ErrorMessage = "Đánh giá phải từ 1 đến 5 sao")]
        [Display(Name = "Đánh giá")]
        public int Rating { get; set; }

        [MaxLength(1000, ErrorMessage = "Nhận xét không được quá 1000 ký tự")]
        [Display(Name = "Nhận xét")]
        public string? Comment { get; set; }

        public int? BorrowRecordId { get; set; } // Để track borrow record nếu cần
    }

    // ViewModel cho cập nhật review
    public class UpdateReviewViewModel
    {
        [Required]
        public int Id { get; set; }

        [Range(1, 5, ErrorMessage = "Đánh giá phải từ 1 đến 5 sao")]
        [Display(Name = "Đánh giá")]
        public int Rating { get; set; }

        [MaxLength(1000, ErrorMessage = "Nhận xét không được quá 1000 ký tự")]
        [Display(Name = "Nhận xét")]
        public string? Comment { get; set; }

        public string BookTitle { get; set; } = string.Empty;
        public string BookCoverUrl { get; set; } = string.Empty;
    }

    // ViewModel cho hiển thị review
    public class ReviewViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserAvatarUrl { get; set; } = string.Empty;
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string BookCoverUrl { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime ReviewDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool CanEdit { get; set; }

        // Helper properties for display
        public string DisplayRating => new string('★', Rating) + new string('☆', 5 - Rating);
        public string TimeAgo
        {
            get
            {
                var timeDiff = DateTime.Now - CreatedAt;
                if (timeDiff.Days > 0)
                    return $"{timeDiff.Days} ngày trước";
                if (timeDiff.Hours > 0)
                    return $"{timeDiff.Hours} giờ trước";
                if (timeDiff.Minutes > 0)
                    return $"{timeDiff.Minutes} phút trước";
                return "Vừa xong";
            }
        }
    }

    // ViewModel cho tổng quan review của sách
    public class BookReviewSummaryViewModel
    {
        public int BookId { get; set; }
        public int TotalReviews { get; set; }
        public double AverageRating { get; set; }
        public int FiveStarCount { get; set; }
        public int FourStarCount { get; set; }
        public int ThreeStarCount { get; set; }
        public int TwoStarCount { get; set; }
        public int OneStarCount { get; set; }
        public List<ReviewViewModel> RecentReviews { get; set; } = new();

        // Helper properties
        public string AverageRatingDisplay => AverageRating.ToString("F1");
        public string DisplayStars => new string('★', (int)Math.Round(AverageRating)) + new string('☆', 5 - (int)Math.Round(AverageRating));

        public int FiveStarPercentage => TotalReviews > 0 ? (FiveStarCount * 100) / TotalReviews : 0;
        public int FourStarPercentage => TotalReviews > 0 ? (FourStarCount * 100) / TotalReviews : 0;
        public int ThreeStarPercentage => TotalReviews > 0 ? (ThreeStarCount * 100) / TotalReviews : 0;
        public int TwoStarPercentage => TotalReviews > 0 ? (TwoStarCount * 100) / TotalReviews : 0;
        public int OneStarPercentage => TotalReviews > 0 ? (OneStarCount * 100) / TotalReviews : 0;
    }

    // ViewModel cho danh sách reviews với phân trang
    public class ReviewListViewModel
    {
        public List<ReviewViewModel> Reviews { get; set; } = new();
        public int TotalCount { get; set; }
        public int TotalReviews => TotalCount;
        public int Page { get; set; }
        public int CurrentPage => Page;
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public double AverageRating { get; set; }
        public Dictionary<int, int> RatingDistribution { get; set; } = new();

        // Pagination helpers
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
        public int PreviousPage => Page - 1;
        public int NextPage => Page + 1;
    }

    // ViewModel cho response sau khi tạo/cập nhật review
    public class ReviewResponseViewModel
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public ReviewViewModel? Review { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    // ViewModel cho kiểm tra điều kiện review
    public class CanReviewViewModel
    {
        public bool CanReview { get; set; }
        public bool HasExistingReview { get; set; }
        public ReviewViewModel? ExistingReview { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}