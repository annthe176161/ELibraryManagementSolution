using ELibraryManagement.Web.Services;
using ELibraryManagement.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace ELibraryManagement.Web.Controllers
{
    public class BorrowController : Controller
    {
        private readonly IBorrowApiService _borrowApiService;

        public BorrowController(IBorrowApiService borrowApiService)
        {
            _borrowApiService = borrowApiService;
        }

        [HttpPost]
        public async Task<IActionResult> ExtendBorrow(int id, string? reason = null)
        {
            if (!await _borrowApiService.IsAuthenticatedAsync())
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để thực hiện chức năng này." });
            }

            try
            {
                var result = await _borrowApiService.ExtendBorrowAsync(id, reason);

                return Json(new
                {
                    success = result.Success,
                    message = result.Message,
                    borrowRecordId = result.BorrowRecordId,
                    bookTitle = result.BookTitle,
                    oldDueDate = result.OldDueDate.ToString("dd/MM/yyyy"),
                    newDueDate = result.NewDueDate.ToString("dd/MM/yyyy"),
                    extensionCount = result.ExtensionCount,
                    remainingExtensions = result.RemainingExtensions
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }
    }
}