using ELibraryManagement.Api.Data;
using ELibraryManagement.Api.DTOs;
using ELibraryManagement.Api.Models;
using ELibraryManagement.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ELibraryManagement.Api.Services.Implementations
{
    public class ReviewService : IReviewService
    {
        private readonly ApplicationDbContext _context;

        public ReviewService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ReviewResponseDto> CreateReviewAsync(string userId, CreateReviewDto createReviewDto)
        {
            try
            {
                // Kiểm tra user có thể review sách này không
                var canReview = await CanUserReviewBookAsync(userId, createReviewDto.BookId);
                if (!canReview)
                {
                    return new ReviewResponseDto
                    {
                        Success = false,
                        Message = "Bạn chỉ có thể đánh giá sách sau khi đã mượn và trả sách.",
                        Errors = new List<string> { "User không đủ điều kiện để review sách này." }
                    };
                }

                // Kiểm tra user đã review sách này chưa
                var existingReview = await GetUserReviewForBookAsync(userId, createReviewDto.BookId);
                if (existingReview != null)
                {
                    return new ReviewResponseDto
                    {
                        Success = false,
                        Message = "Bạn đã đánh giá sách này rồi. Bạn có thể chỉnh sửa đánh giá hiện tại.",
                        Errors = new List<string> { "Đã có review từ user này cho sách này." }
                    };
                }

                // Kiểm tra sách có tồn tại không
                var book = await _context.Books.FindAsync(createReviewDto.BookId);
                if (book == null)
                {
                    return new ReviewResponseDto
                    {
                        Success = false,
                        Message = "Không tìm thấy sách.",
                        Errors = new List<string> { "Sách không tồn tại." }
                    };
                }

                // Tạo review mới
                var review = new Review
                {
                    UserId = userId,
                    BookId = createReviewDto.BookId,
                    Rating = createReviewDto.Rating,
                    Comment = createReviewDto.Comment,
                    ReviewDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();

                // Lấy thông tin review vừa tạo
                var reviewDto = await GetReviewByIdAsync(review.Id);

                return new ReviewResponseDto
                {
                    Success = true,
                    Message = "Đánh giá sách thành công!",
                    Review = reviewDto
                };
            }
            catch (Exception ex)
            {
                return new ReviewResponseDto
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi tạo đánh giá.",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ReviewResponseDto> UpdateReviewAsync(string userId, int reviewId, UpdateReviewDto updateReviewDto)
        {
            try
            {
                var review = await _context.Reviews.FindAsync(reviewId);
                if (review == null)
                {
                    return new ReviewResponseDto
                    {
                        Success = false,
                        Message = "Không tìm thấy đánh giá.",
                        Errors = new List<string> { "Review không tồn tại." }
                    };
                }

                // Kiểm tra quyền sở hữu
                if (review.UserId != userId)
                {
                    return new ReviewResponseDto
                    {
                        Success = false,
                        Message = "Bạn không có quyền chỉnh sửa đánh giá này.",
                        Errors = new List<string> { "Không có quyền truy cập." }
                    };
                }

                // Cập nhật review
                review.Rating = updateReviewDto.Rating;
                review.Comment = updateReviewDto.Comment;
                review.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var reviewDto = await GetReviewByIdAsync(reviewId);

                return new ReviewResponseDto
                {
                    Success = true,
                    Message = "Cập nhật đánh giá thành công!",
                    Review = reviewDto
                };
            }
            catch (Exception ex)
            {
                return new ReviewResponseDto
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi cập nhật đánh giá.",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ReviewResponseDto> DeleteReviewAsync(string userId, int reviewId)
        {
            try
            {
                var review = await _context.Reviews.FindAsync(reviewId);
                if (review == null)
                {
                    return new ReviewResponseDto
                    {
                        Success = false,
                        Message = "Không tìm thấy đánh giá.",
                        Errors = new List<string> { "Review không tồn tại." }
                    };
                }

                // Kiểm tra quyền sở hữu
                if (review.UserId != userId)
                {
                    return new ReviewResponseDto
                    {
                        Success = false,
                        Message = "Bạn không có quyền xóa đánh giá này.",
                        Errors = new List<string> { "Không có quyền truy cập." }
                    };
                }

                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();

                return new ReviewResponseDto
                {
                    Success = true,
                    Message = "Xóa đánh giá thành công!"
                };
            }
            catch (Exception ex)
            {
                return new ReviewResponseDto
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi xóa đánh giá.",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ReviewResponseDto> DeleteReviewByAdminAsync(int reviewId)
        {
            try
            {
                var review = await _context.Reviews.FindAsync(reviewId);
                if (review == null)
                {
                    return new ReviewResponseDto
                    {
                        Success = false,
                        Message = "Không tìm thấy đánh giá.",
                        Errors = new List<string> { "Review không tồn tại." }
                    };
                }

                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();

                return new ReviewResponseDto
                {
                    Success = true,
                    Message = "Xóa đánh giá thành công!"
                };
            }
            catch (Exception ex)
            {
                return new ReviewResponseDto
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi xóa đánh giá.",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ReviewDto?> GetReviewByIdAsync(int reviewId)
        {
            var review = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Book)
                .FirstOrDefaultAsync(r => r.Id == reviewId);

            if (review == null) return null;

            return new ReviewDto
            {
                Id = review.Id,
                UserId = review.UserId,
                UserName = $"{review.User.FirstName} {review.User.LastName}".Trim(),
                UserEmail = review.User.Email ?? "",
                UserAvatarUrl = review.User.AvatarUrl ?? "",
                BookId = review.BookId,
                BookTitle = review.Book.Title,
                BookAuthor = review.Book.Author,
                BookCoverUrl = review.Book.CoverImageUrl ?? "",
                Rating = review.Rating,
                Comment = review.Comment,
                ReviewDate = review.ReviewDate,
                CreatedAt = review.CreatedAt,
                UpdatedAt = review.UpdatedAt
            };
        }

        public async Task<(List<ReviewDto> Reviews, int TotalCount)> GetBookReviewsAsync(int bookId, int page = 1, int pageSize = 10)
        {
            var query = _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Book)
                .Where(r => r.BookId == bookId)
                .OrderByDescending(r => r.CreatedAt);

            var totalCount = await query.CountAsync();

            var reviews = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    UserName = $"{r.User.FirstName} {r.User.LastName}".Trim(),
                    UserAvatarUrl = r.User.AvatarUrl ?? "",
                    BookId = r.BookId,
                    BookTitle = r.Book.Title,
                    BookCoverUrl = r.Book.CoverImageUrl ?? "",
                    Rating = r.Rating,
                    Comment = r.Comment,
                    ReviewDate = r.ReviewDate,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt
                })
                .ToListAsync();

            return (reviews, totalCount);
        }

        public async Task<BookReviewSummaryDto> GetBookReviewSummaryAsync(int bookId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.BookId == bookId)
                .ToListAsync();

            var summary = new BookReviewSummaryDto
            {
                BookId = bookId,
                TotalReviews = reviews.Count,
                AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0,
                FiveStarCount = reviews.Count(r => r.Rating == 5),
                FourStarCount = reviews.Count(r => r.Rating == 4),
                ThreeStarCount = reviews.Count(r => r.Rating == 3),
                TwoStarCount = reviews.Count(r => r.Rating == 2),
                OneStarCount = reviews.Count(r => r.Rating == 1)
            };

            // Lấy 5 review gần nhất
            var (recentReviews, _) = await GetBookReviewsAsync(bookId, 1, 5);
            summary.RecentReviews = recentReviews;

            return summary;
        }

        public async Task<List<ReviewDto>> GetUserReviewsAsync(string userId)
        {
            var reviews = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Book)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    UserName = $"{r.User.FirstName} {r.User.LastName}".Trim(),
                    UserAvatarUrl = r.User.AvatarUrl ?? "",
                    BookId = r.BookId,
                    BookTitle = r.Book.Title,
                    BookCoverUrl = r.Book.CoverImageUrl ?? "",
                    Rating = r.Rating,
                    Comment = r.Comment,
                    ReviewDate = r.ReviewDate,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt
                })
                .ToListAsync();

            return reviews;
        }

        public async Task<bool> CanUserReviewBookAsync(string userId, int bookId)
        {
            // Kiểm tra user đã từng mượn và trả sách này
            var hasReturnedBook = await _context.BorrowRecords
                .AnyAsync(br => br.UserId == userId &&
                               br.BookId == bookId &&
                               br.ReturnDate.HasValue &&
                               br.Status == BorrowStatus.Returned);

            return hasReturnedBook;
        }

        public async Task<ReviewDto?> GetUserReviewForBookAsync(string userId, int bookId)
        {
            var review = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Book)
                .FirstOrDefaultAsync(r => r.UserId == userId && r.BookId == bookId);

            if (review == null) return null;

            return new ReviewDto
            {
                Id = review.Id,
                UserId = review.UserId,
                UserName = $"{review.User.FirstName} {review.User.LastName}".Trim(),
                UserAvatarUrl = review.User.AvatarUrl ?? "",
                BookId = review.BookId,
                BookTitle = review.Book.Title,
                BookCoverUrl = review.Book.CoverImageUrl ?? "",
                Rating = review.Rating,
                Comment = review.Comment,
                ReviewDate = review.ReviewDate,
                CreatedAt = review.CreatedAt,
                UpdatedAt = review.UpdatedAt,
                CanEdit = true // Vì đây là review của chính user này
            };
        }

        public async Task<List<ReviewDto>> GetAllReviewsAsync()
        {
            try
            {
                var reviews = await _context.Reviews
                    .Include(r => r.User)
                    .Include(r => r.Book)
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => new ReviewDto
                    {
                        Id = r.Id,
                        UserId = r.UserId,
                        UserName = $"{r.User.FirstName} {r.User.LastName}".Trim(),
                        UserEmail = r.User.Email ?? "",
                        UserAvatarUrl = r.User.AvatarUrl ?? "",
                        BookId = r.BookId,
                        BookTitle = r.Book.Title,
                        BookAuthor = r.Book.Author,
                        BookCoverUrl = r.Book.CoverImageUrl ?? "",
                        Rating = r.Rating,
                        Comment = r.Comment,
                        ReviewDate = r.ReviewDate,
                        CreatedAt = r.CreatedAt,
                        UpdatedAt = r.UpdatedAt,
                        CanEdit = false // Admin view, not user's own review
                    })
                    .ToListAsync();

                return reviews;
            }
            catch (Exception)
            {
                return new List<ReviewDto>();
            }
        }
    }
}