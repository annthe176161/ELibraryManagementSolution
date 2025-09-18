using ELibraryManagement.Web.Services;
using ELibraryManagement.Web.Models;
using ELibraryManagement.Web.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace ELibraryManagement.Web.Controllers
{
    public class BorrowController : Controller
    {
        private readonly IBorrowApiService _borrowApiService;
        private readonly IBookApiService _bookApiService;
        private readonly IAuthApiService _authApiService;

        public BorrowController(IBorrowApiService borrowApiService, IBookApiService bookApiService, IAuthApiService authApiService)
        {
            _borrowApiService = borrowApiService;
            _bookApiService = bookApiService;
            _authApiService = authApiService;
        }

        public async Task<IActionResult> BorrowBook(int bookId)
        {
            // Kiểm tra authentication thủ công
            var isAuthenticated = await _borrowApiService.IsAuthenticatedAsync();

            if (!isAuthenticated)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để mượn sách.";
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("BorrowBook", "Borrow", new { bookId }) });
            }

            try
            {
                // Lấy thông tin sách
                var book = await _bookApiService.GetBookByIdAsync(bookId);
                if (book == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy sách.";
                    return RedirectToAction("Books", "Home");
                }

                // Lấy thông tin user hiện tại
                var currentUser = await _authApiService.GetCurrentUserAsync();
                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Tạo ViewModel
                var viewModel = new BorrowConfirmationViewModel
                {
                    Book = book,
                    Student = new StudentInfoViewModel
                    {
                        FullName = $"{currentUser.FirstName} {currentUser.LastName}".Trim(),
                        Email = currentUser.Email,
                        StudentId = currentUser.StudentId ?? "Chưa cập nhật",
                        PhoneNumber = currentUser.PhoneNumber ?? "Chưa cập nhật",
                        Faculty = "Chưa cập nhật", // Sẽ cần thêm vào UserViewModel sau
                        Class = "Chưa cập nhật" // Sẽ cần thêm vào UserViewModel sau
                    },
                    BorrowDate = DateTimeHelper.VietnamNow(),
                    DueDate = DateTimeHelper.VietnamNow().AddDays(30), // 30 ngày như quy định mới
                    MaxExtensions = 2
                };

                return View(viewModel);
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin sách.";
                return RedirectToAction("Books", "Home");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmBorrow(int bookId)
        {
            // Kiểm tra authentication thủ công
            if (!await _borrowApiService.IsAuthenticatedAsync())
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để mượn sách.";
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var result = await _borrowApiService.BorrowBookAsync(bookId);
                if (result.Success)
                {
                    TempData["SuccessMessage"] = "Mượn sách thành công! Bạn có thể xem chi tiết trong lịch sử mượn sách.";
                    return RedirectToAction("MyBorrows", "Borrow");
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message;
                    return RedirectToAction("BorrowBook", new { bookId });
                }
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi mượn sách.";
                return RedirectToAction("BorrowBook", new { bookId });
            }
        }

        public async Task<IActionResult> MyBorrows()
        {
            // Kiểm tra authentication thủ công
            if (!await _borrowApiService.IsAuthenticatedAsync())
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem lịch sử mượn sách.";
                return RedirectToAction("Login", "Account");
            }

            // Placeholder - sẽ implement sau
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult ExtendBorrow(int id, string? reason = null)
        {
            return Json(new { success = false, message = "Chức năng gia hạn sách đã bị vô hiệu hóa." });
        }
    }
}
