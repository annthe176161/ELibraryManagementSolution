using ELibraryManagement.Api.DTOs;
using ELibraryManagement.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ELibraryManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        /// <summary>
        /// Tạo review mới cho sách - Yêu cầu đăng nhập
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto createReviewDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Không thể xác định user.");
            }

            var result = await _reviewService.CreateReviewAsync(userId, createReviewDto);

            if (result.Success)
            {
                return CreatedAtAction(nameof(GetReviewById), new { id = result.Review?.Id }, result);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Cập nhật review - Yêu cầu đăng nhập và là chủ review
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReview(int id, [FromBody] UpdateReviewDto updateReviewDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Không thể xác định user.");
            }

            var result = await _reviewService.UpdateReviewAsync(userId, id, updateReviewDto);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Xóa review - Yêu cầu đăng nhập và là chủ review hoặc admin
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Không thể xác định user.");
            }

            // Admin có thể xóa bất kỳ review nào
            if (userRole == "Admin")
            {
                var adminResult = await _reviewService.DeleteReviewByAdminAsync(id);
                if (adminResult.Success)
                {
                    return Ok(adminResult);
                }
                return BadRequest(adminResult);
            }

            // User thường chỉ có thể xóa review của chính họ
            var result = await _reviewService.DeleteReviewAsync(userId, id);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Lấy review theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReviewById(int id)
        {
            var review = await _reviewService.GetReviewByIdAsync(id);

            if (review == null)
            {
                return NotFound("Không tìm thấy review.");
            }

            // Kiểm tra quyền edit nếu user đã đăng nhập
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            review.CanEdit = !string.IsNullOrEmpty(userId) && review.UserId == userId;

            return Ok(review);
        }

        /// <summary>
        /// Lấy danh sách reviews của một sách với phân trang
        /// </summary>
        [HttpGet("book/{bookId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBookReviews(int bookId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 50) pageSize = 10;

            var (reviews, totalCount) = await _reviewService.GetBookReviewsAsync(bookId, page, pageSize);

            // Kiểm tra quyền edit cho từng review nếu user đã đăng nhập
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                foreach (var review in reviews)
                {
                    review.CanEdit = review.UserId == userId;
                }
            }

            var response = new
            {
                Reviews = reviews,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            return Ok(response);
        }

        /// <summary>
        /// Lấy tổng quan review của một sách
        /// </summary>
        [HttpGet("book/{bookId}/summary")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBookReviewSummary(int bookId)
        {
            var summary = await _reviewService.GetBookReviewSummaryAsync(bookId);

            // Kiểm tra quyền edit cho recent reviews nếu user đã đăng nhập
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                foreach (var review in summary.RecentReviews)
                {
                    review.CanEdit = review.UserId == userId;
                }
            }

            return Ok(summary);
        }

        /// <summary>
        /// Lấy danh sách reviews của user hiện tại
        /// </summary>
        [HttpGet("my-reviews")]
        public async Task<IActionResult> GetMyReviews()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Không thể xác định user.");
            }

            var reviews = await _reviewService.GetUserReviewsAsync(userId);

            // Tất cả reviews đều có thể edit vì là của chính user
            foreach (var review in reviews)
            {
                review.CanEdit = true;
            }

            return Ok(reviews);
        }

        /// <summary>
        /// Kiểm tra user có thể review sách này không
        /// </summary>
        [HttpGet("can-review/{bookId}")]
        public async Task<IActionResult> CanReviewBook(int bookId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Không thể xác định user.");
            }

            var canReview = await _reviewService.CanUserReviewBookAsync(userId, bookId);
            var existingReview = await _reviewService.GetUserReviewForBookAsync(userId, bookId);

            var response = new
            {
                CanReview = canReview,
                HasExistingReview = existingReview != null,
                ExistingReview = existingReview
            };

            return Ok(response);
        }

        /// <summary>
        /// Lấy review của user cho một sách cụ thể
        /// </summary>
        [HttpGet("my-review/{bookId}")]
        public async Task<IActionResult> GetMyReviewForBook(int bookId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Không thể xác định user.");
            }

            var review = await _reviewService.GetUserReviewForBookAsync(userId, bookId);

            if (review == null)
            {
                return NotFound("Bạn chưa đánh giá sách này.");
            }

            return Ok(review);
        }

        /// <summary>
        /// Lấy tất cả reviews cho admin
        /// </summary>
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllReviews()
        {
            var reviews = await _reviewService.GetAllReviewsAsync();
            return Ok(reviews);
        }
    }
}