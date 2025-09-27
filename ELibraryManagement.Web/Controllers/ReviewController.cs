using ELibraryManagement.Web.Models;
using ELibraryManagement.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ELibraryManagement.Web.Controllers
{
    public class ReviewController : Controller
    {
        private readonly IReviewApiService _reviewApiService;
        private readonly IBookApiService _bookApiService;
        private readonly IAuthApiService _authApiService;

        public ReviewController(
            IReviewApiService reviewApiService,
            IBookApiService bookApiService,
            IAuthApiService authApiService)
        {
            _reviewApiService = reviewApiService;
            _bookApiService = bookApiService;
            _authApiService = authApiService;
        }

        // Helper method to check authentication
        private bool IsUserAuthenticated()
        {
            return _authApiService.IsAuthenticated();
        }

        private IActionResult? RedirectToLoginIfNotAuthenticated()
        {
            if (!IsUserAuthenticated())
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để sử dụng tính năng này.";
                return RedirectToAction("Login", "Account");
            }
            return null;
        }

        // GET: Review/Create/5
        public async Task<IActionResult> Create(int bookId, int? borrowRecordId)
        {
            var authCheck = RedirectToLoginIfNotAuthenticated();
            if (authCheck != null) return authCheck;

            try
            {
                var token = _authApiService.GetCurrentToken();
                if (string.IsNullOrEmpty(token))
                {
                    TempData["ErrorMessage"] = "Phiên đăng nhập đã hết hạn.";
                    return RedirectToAction("Login", "Account");
                }

                // Kiểm tra điều kiện có thể review
                var canReview = await _reviewApiService.CanReviewBookAsync(bookId, token);
                if (!canReview.CanReview)
                {
                    TempData["ErrorMessage"] = canReview.Message;
                    return RedirectToAction("BookDetail", "Home", new { id = bookId });
                }

                if (canReview.HasExistingReview)
                {
                    TempData["InfoMessage"] = "Bạn đã đánh giá sách này rồi. Bạn có thể chỉnh sửa đánh giá hiện tại.";
                    return RedirectToAction("Edit", new { id = canReview.ExistingReview?.Id });
                }

                // Lấy thông tin sách
                var book = await _bookApiService.GetBookByIdAsync(bookId);
                if (book == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy sách.";
                    return RedirectToAction("Index", "Home");
                }

                var model = new CreateReviewViewModel
                {
                    BookId = bookId,
                    BookTitle = book.Title,
                    BookAuthor = book.Author,
                    BookCoverUrl = book.ImageUrl,
                    BorrowRecordId = borrowRecordId
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: Review/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateReviewViewModel model)
        {
            var authCheck = RedirectToLoginIfNotAuthenticated();
            if (authCheck != null) return authCheck;

            if (!ModelState.IsValid)
            {
                // Reload book info nếu model không hợp lệ
                var book = await _bookApiService.GetBookByIdAsync(model.BookId);
                if (book != null)
                {
                    model.BookTitle = book.Title;
                    model.BookAuthor = book.Author;
                    model.BookCoverUrl = book.ImageUrl;
                }
                return View(model);
            }

            try
            {
                var token = _authApiService.GetCurrentToken();
                if (string.IsNullOrEmpty(token))
                {
                    TempData["ErrorMessage"] = "Phiên đăng nhập đã hết hạn.";
                    return RedirectToAction("Login", "Account");
                }

                var result = await _reviewApiService.CreateReviewAsync(model, token);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction("BookDetail", "Home", new { id = model.BookId });
                }

                TempData["ErrorMessage"] = result.Message;
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
                return View(model);
            }
        }

        // GET: Review/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var authCheck = RedirectToLoginIfNotAuthenticated();
            if (authCheck != null) return authCheck;

            try
            {
                var token = _authApiService.GetCurrentToken();
                if (string.IsNullOrEmpty(token))
                {
                    TempData["ErrorMessage"] = "Phiên đăng nhập đã hết hạn.";
                    return RedirectToAction("Login", "Account");
                }

                var review = await _reviewApiService.GetReviewByIdAsync(id, token);
                if (review == null)
                {
                    // TempData["ErrorMessage"] = "Không tìm thấy đánh giá.";
                    return RedirectToAction("MyReviews");
                }

                if (!review.CanEdit)
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền chỉnh sửa đánh giá này.";
                    return RedirectToAction("BookDetail", "Home", new { id = review.BookId });
                }

                var model = new UpdateReviewViewModel
                {
                    Id = review.Id,
                    Rating = review.Rating,
                    Comment = review.Comment,
                    BookTitle = review.BookTitle,
                    BookCoverUrl = review.BookCoverUrl
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
                return RedirectToAction("MyReviews");
            }
        }

        // POST: Review/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateReviewViewModel model)
        {
            var authCheck = RedirectToLoginIfNotAuthenticated();
            if (authCheck != null) return authCheck;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var token = _authApiService.GetCurrentToken();
                if (string.IsNullOrEmpty(token))
                {
                    TempData["ErrorMessage"] = "Phiên đăng nhập đã hết hạn.";
                    return RedirectToAction("Login", "Account");
                }

                var result = await _reviewApiService.UpdateReviewAsync(model, token);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction("MyReviews");
                }

                TempData["ErrorMessage"] = result.Message;
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
                return View(model);
            }
        }

        // POST: Review/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var authCheck = RedirectToLoginIfNotAuthenticated();
            if (authCheck != null) return Json(new { success = false, message = "Bạn cần đăng nhập để sử dụng tính năng này." });

            try
            {
                var token = _authApiService.GetCurrentToken();
                if (string.IsNullOrEmpty(token))
                {
                    return Json(new { success = false, message = "Phiên đăng nhập đã hết hạn." });
                }

                var result = await _reviewApiService.DeleteReviewAsync(id, token);

                if (result.Success)
                {
                    return Json(new { success = true, message = result.Message });
                }

                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        // GET: Review/MyReviews
        public async Task<IActionResult> MyReviews()
        {
            var authCheck = RedirectToLoginIfNotAuthenticated();
            if (authCheck != null) return authCheck;

            try
            {
                var token = _authApiService.GetCurrentToken();
                if (string.IsNullOrEmpty(token))
                {
                    TempData["ErrorMessage"] = "Phiên đăng nhập đã hết hạn.";
                    return RedirectToAction("Login", "Account");
                }

                var reviews = await _reviewApiService.GetMyReviewsAsync(token);
                return View(reviews);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
                return View(new List<ReviewViewModel>());
            }
        }

        // GET: Review/BookReviews/5
        public async Task<IActionResult> BookReviews(int bookId, int page = 1)
        {
            try
            {
                var reviews = await _reviewApiService.GetBookReviewsAsync(bookId, page, 10);

                // Lấy thông tin sách cho title
                var book = await _bookApiService.GetBookByIdAsync(bookId);
                if (book != null)
                {
                    reviews.BookTitle = book.Title;
                }

                return View(reviews);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
                return RedirectToAction("BookDetail", "Home", new { id = bookId });
            }
        }
    }
}
