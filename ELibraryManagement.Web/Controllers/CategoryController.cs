using ELibraryManagement.Web.Models.ViewModels;
using ELibraryManagement.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ELibraryManagement.Web.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryApiService _categoryApiService;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(ICategoryApiService categoryApiService, ILogger<CategoryController> logger)
        {
            _categoryApiService = categoryApiService;
            _logger = logger;
        }

        // GET: Category
        public async Task<IActionResult> Index(bool includeInactive = false)
        {
            try
            {
                // Check if user is admin
                var userRole = HttpContext.Session.GetString("UserRole");
                if (userRole != "Admin")
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này.";
                    return RedirectToAction("Index", "Home");
                }

                var result = await _categoryApiService.GetAllCategoriesAsync(includeInactive);

                if (result.Success)
                {
                    var viewModel = CategoryListViewModel.FromDto(result);
                    viewModel.IncludeInactive = includeInactive;
                    return View(viewModel);
                }

                TempData["ErrorMessage"] = result.Message;
                return View(new CategoryListViewModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading categories");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách danh mục.";
                return View(new CategoryListViewModel());
            }
        }

        // GET: Category/Create
        public IActionResult Create()
        {
            // Check if user is admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToAction("Index", "Home");
            }

            return View(new CreateCategoryViewModel());
        }

        // POST: Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCategoryViewModel model)
        {
            try
            {
                // Check if user is admin
                var userRole = HttpContext.Session.GetString("UserRole");
                if (userRole != "Admin")
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này.";
                    return RedirectToAction("Index", "Home");
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Check if category name already exists
                var nameExists = await _categoryApiService.CheckCategoryNameExistsAsync(model.Name);
                if (nameExists)
                {
                    ModelState.AddModelError("Name", "Tên danh mục đã tồn tại");
                    return View(model);
                }

                var result = await _categoryApiService.CreateCategoryAsync(model.ToDto());

                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction(nameof(Index));
                }

                TempData["ErrorMessage"] = result.Message;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tạo danh mục.";
                return View(model);
            }
        }

        // GET: Category/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                // Check if user is admin
                var userRole = HttpContext.Session.GetString("UserRole");
                if (userRole != "Admin")
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này.";
                    return RedirectToAction("Index", "Home");
                }

                var result = await _categoryApiService.GetCategoryByIdAsync(id);

                if (result.Success && result.Category != null)
                {
                    var viewModel = EditCategoryViewModel.FromDto(result.Category);
                    return View(viewModel);
                }

                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading category for edit: {CategoryId}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin danh mục.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Category/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditCategoryViewModel model)
        {
            try
            {
                // Check if user is admin
                var userRole = HttpContext.Session.GetString("UserRole");
                if (userRole != "Admin")
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này.";
                    return RedirectToAction("Index", "Home");
                }

                if (id != model.Id)
                {
                    TempData["ErrorMessage"] = "Thông tin không hợp lệ.";
                    return RedirectToAction(nameof(Index));
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Check if category name already exists (excluding current category)
                var nameExists = await _categoryApiService.CheckCategoryNameExistsAsync(model.Name, id);
                if (nameExists)
                {
                    ModelState.AddModelError("Name", "Tên danh mục đã tồn tại");
                    return View(model);
                }

                var result = await _categoryApiService.UpdateCategoryAsync(id, model.ToDto());

                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction(nameof(Index));
                }

                TempData["ErrorMessage"] = result.Message;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category: {CategoryId}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật danh mục.";
                return View(model);
            }
        }

        // GET: Category/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Check if user is admin
                var userRole = HttpContext.Session.GetString("UserRole");
                if (userRole != "Admin")
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này.";
                    return RedirectToAction("Index", "Home");
                }

                var result = await _categoryApiService.GetCategoryByIdAsync(id);

                if (result.Success && result.Category != null)
                {
                    var viewModel = CategoryDeleteViewModel.FromDto(result.Category);
                    return View(viewModel);
                }

                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading category for delete: {CategoryId}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin danh mục.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                // Check if user is admin
                var userRole = HttpContext.Session.GetString("UserRole");
                if (userRole != "Admin")
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này.";
                    return RedirectToAction("Index", "Home");
                }

                var result = await _categoryApiService.DeleteCategoryAsync(id);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message;
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category: {CategoryId}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa danh mục.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Category/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                // Check if user is admin
                var userRole = HttpContext.Session.GetString("UserRole");
                if (userRole != "Admin")
                {
                    return Json(new { success = false, message = "Bạn không có quyền thực hiện thao tác này." });
                }

                var result = await _categoryApiService.ToggleCategoryStatusAsync(id);

                return Json(new
                {
                    success = result.Success,
                    message = result.Message,
                    isActive = result.Category?.IsActive ?? false
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling category status: {CategoryId}", id);
                return Json(new { success = false, message = "Có lỗi xảy ra khi thay đổi trạng thái danh mục." });
            }
        }

        // AJAX endpoint to check category name
        [HttpGet]
        public async Task<IActionResult> CheckCategoryName(string name, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return Json(new { exists = false });
                }

                var exists = await _categoryApiService.CheckCategoryNameExistsAsync(name, excludeId);
                return Json(new { exists = exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking category name: {CategoryName}", name);
                return Json(new { exists = false });
            }
        }
    }
}