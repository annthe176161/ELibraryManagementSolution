using ELibraryManagement.Api.Data;
using ELibraryManagement.Api.DTOs;
using ELibraryManagement.Api.Models;
using ELibraryManagement.Api.Services.Interfaces;
using ELibraryManagement.Api.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ELibraryManagement.Api.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(ApplicationDbContext context, ILogger<CategoryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<CategoriesListResponseDto> GetAllCategoriesAsync(bool includeInactive = false)
        {
            try
            {
                var query = _context.Categories.AsQueryable();

                if (!includeInactive)
                {
                    query = query.Where(c => c.IsActive);
                }

                var categories = await query
                    .OrderBy(c => c.Name)
                    .Select(c => new CategoryDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description,
                        Color = c.Color,
                        IsActive = c.IsActive,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt,
                        BookCount = c.BookCategories.Count(bc => !bc.Book.IsDeleted)
                    })
                    .ToListAsync();

                return new CategoriesListResponseDto
                {
                    Success = true,
                    Message = "Lấy danh sách danh mục thành công",
                    Categories = categories,
                    TotalCount = categories.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting categories");
                return new CategoriesListResponseDto
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách danh mục"
                };
            }
        }

        public async Task<CategoryResponseDto> GetCategoryByIdAsync(int id)
        {
            try
            {
                var category = await _context.Categories
                    .Where(c => c.Id == id)
                    .Select(c => new CategoryDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description,
                        Color = c.Color,
                        IsActive = c.IsActive,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt,
                        BookCount = c.BookCategories.Count(bc => !bc.Book.IsDeleted)
                    })
                    .FirstOrDefaultAsync();

                if (category == null)
                {
                    return new CategoryResponseDto
                    {
                        Success = false,
                        Message = "Không tìm thấy danh mục"
                    };
                }

                return new CategoryResponseDto
                {
                    Success = true,
                    Message = "Lấy thông tin danh mục thành công",
                    Category = category
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category by id {CategoryId}", id);
                return new CategoryResponseDto
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi lấy thông tin danh mục"
                };
            }
        }

        public async Task<CategoryResponseDto> CreateCategoryAsync(CreateCategoryDto createDto)
        {
            try
            {
                // Kiểm tra tên danh mục đã tồn tại chưa
                if (await CategoryExistsAsync(createDto.Name))
                {
                    return new CategoryResponseDto
                    {
                        Success = false,
                        Message = "Tên danh mục đã tồn tại"
                    };
                }

                var category = new Category
                {
                    Name = createDto.Name.Trim(),
                    Description = createDto.Description?.Trim(),
                    Color = createDto.Color?.Trim(),
                    IsActive = createDto.IsActive,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                var categoryDto = new CategoryDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    Color = category.Color,
                    IsActive = category.IsActive,
                    CreatedAt = category.CreatedAt,
                    UpdatedAt = category.UpdatedAt,
                    BookCount = 0
                };

                return new CategoryResponseDto
                {
                    Success = true,
                    Message = "Tạo danh mục thành công",
                    Category = categoryDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                return new CategoryResponseDto
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi tạo danh mục"
                };
            }
        }

        public async Task<CategoryResponseDto> UpdateCategoryAsync(int id, UpdateCategoryDto updateDto)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                {
                    return new CategoryResponseDto
                    {
                        Success = false,
                        Message = "Không tìm thấy danh mục"
                    };
                }

                // Kiểm tra tên danh mục đã tồn tại chưa (trừ chính nó)
                if (await CategoryExistsAsync(updateDto.Name, id))
                {
                    return new CategoryResponseDto
                    {
                        Success = false,
                        Message = "Tên danh mục đã tồn tại"
                    };
                }

                category.Name = updateDto.Name.Trim();
                category.Description = updateDto.Description?.Trim();
                category.Color = updateDto.Color?.Trim();
                category.IsActive = updateDto.IsActive;
                category.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var categoryDto = new CategoryDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    Color = category.Color,
                    IsActive = category.IsActive,
                    CreatedAt = category.CreatedAt,
                    UpdatedAt = category.UpdatedAt,
                    BookCount = await _context.BookCategories
                        .CountAsync(bc => bc.CategoryId == id && !bc.Book.IsDeleted)
                };

                return new CategoryResponseDto
                {
                    Success = true,
                    Message = "Cập nhật danh mục thành công",
                    Category = categoryDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category {CategoryId}", id);
                return new CategoryResponseDto
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi cập nhật danh mục"
                };
            }
        }

        public async Task<CategoryResponseDto> DeleteCategoryAsync(int id)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.BookCategories)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                {
                    return new CategoryResponseDto
                    {
                        Success = false,
                        Message = "Không tìm thấy danh mục"
                    };
                }

                // Kiểm tra có sách nào đang sử dụng danh mục này không
                var hasActiveBooks = await _context.BookCategories
                    .AnyAsync(bc => bc.CategoryId == id && !bc.Book.IsDeleted);

                if (hasActiveBooks)
                {
                    return new CategoryResponseDto
                    {
                        Success = false,
                        Message = "Không thể xóa danh mục vì còn sách đang sử dụng"
                    };
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                return new CategoryResponseDto
                {
                    Success = true,
                    Message = "Xóa danh mục thành công"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category {CategoryId}", id);
                return new CategoryResponseDto
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi xóa danh mục"
                };
            }
        }

        public async Task<CategoryResponseDto> ToggleCategoryStatusAsync(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                {
                    return new CategoryResponseDto
                    {
                        Success = false,
                        Message = "Không tìm thấy danh mục"
                    };
                }

                category.IsActive = !category.IsActive;
                category.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var categoryDto = new CategoryDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    Color = category.Color,
                    IsActive = category.IsActive,
                    CreatedAt = category.CreatedAt,
                    UpdatedAt = category.UpdatedAt,
                    BookCount = await _context.BookCategories
                        .CountAsync(bc => bc.CategoryId == id && !bc.Book.IsDeleted)
                };

                return new CategoryResponseDto
                {
                    Success = true,
                    Message = $"Đã {(category.IsActive ? "kích hoạt" : "vô hiệu hóa")} danh mục",
                    Category = categoryDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling category status {CategoryId}", id);
                return new CategoryResponseDto
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi thay đổi trạng thái danh mục"
                };
            }
        }

        public async Task<bool> CategoryExistsAsync(string name, int? excludeId = null)
        {
            try
            {
                var query = _context.Categories.Where(c => c.Name.ToLower() == name.ToLower().Trim());

                if (excludeId.HasValue)
                {
                    query = query.Where(c => c.Id != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if category exists with name {CategoryName}", name);
                return false;
            }
        }
    }
}