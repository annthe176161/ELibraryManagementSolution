using ELibraryManagement.Api.DTOs;
using ELibraryManagement.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELibraryManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(ICategoryService categoryService, ILogger<CategoriesController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách tất cả danh mục
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllCategories([FromQuery] bool includeInactive = false)
        {
            try
            {
                var result = await _categoryService.GetAllCategoriesAsync(includeInactive);

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all categories");
                return StatusCode(500, new { message = "Có lỗi xảy ra khi lấy danh sách danh mục" });
            }
        }

        /// <summary>
        /// Lấy thông tin danh mục theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            try
            {
                var result = await _categoryService.GetCategoryByIdAsync(id);

                if (result.Success)
                {
                    return Ok(result);
                }

                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category by id {CategoryId}", id);
                return StatusCode(500, new { message = "Có lỗi xảy ra khi lấy thông tin danh mục" });
            }
        }

        /// <summary>
        /// Tạo danh mục mới - Chỉ dành cho Admin
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _categoryService.CreateCategoryAsync(createDto);

                if (result.Success)
                {
                    return CreatedAtAction(
                        nameof(GetCategoryById),
                        new { id = result.Category!.Id },
                        result
                    );
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                return StatusCode(500, new { message = "Có lỗi xảy ra khi tạo danh mục" });
            }
        }

        /// <summary>
        /// Cập nhật danh mục - Chỉ dành cho Admin
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _categoryService.UpdateCategoryAsync(id, updateDto);

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category {CategoryId}", id);
                return StatusCode(500, new { message = "Có lỗi xảy ra khi cập nhật danh mục" });
            }
        }

        /// <summary>
        /// Xóa danh mục - Chỉ dành cho Admin
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var result = await _categoryService.DeleteCategoryAsync(id);

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category {CategoryId}", id);
                return StatusCode(500, new { message = "Có lỗi xảy ra khi xóa danh mục" });
            }
        }

        /// <summary>
        /// Bật/tắt trạng thái danh mục - Chỉ dành cho Admin
        /// </summary>
        [HttpPatch("{id}/toggle-status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleCategoryStatus(int id)
        {
            try
            {
                var result = await _categoryService.ToggleCategoryStatusAsync(id);

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling category status {CategoryId}", id);
                return StatusCode(500, new { message = "Có lỗi xảy ra khi thay đổi trạng thái danh mục" });
            }
        }

        /// <summary>
        /// Kiểm tra tên danh mục đã tồn tại - Chỉ dành cho Admin
        /// </summary>
        [HttpGet("check-name")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CheckCategoryName([FromQuery] string name, [FromQuery] int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(new { message = "Tên danh mục không được để trống" });
            }

            try
            {
                var exists = await _categoryService.CategoryExistsAsync(name, excludeId);

                return Ok(new { exists = exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking category name");
                return StatusCode(500, new { message = "Có lỗi xảy ra khi kiểm tra tên danh mục" });
            }
        }
    }
}