using System.ComponentModel.DataAnnotations;

namespace ELibraryManagement.Api.DTOs
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Color { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int BookCount { get; set; }
    }

    public class CreateCategoryDto
    {
        [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên danh mục không được vượt quá 100 ký tự")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        public string? Description { get; set; }

        [StringLength(20, ErrorMessage = "Mã màu không được vượt quá 20 ký tự")]
        public string? Color { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateCategoryDto
    {
        [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên danh mục không được vượt quá 100 ký tự")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        public string? Description { get; set; }

        [StringLength(20, ErrorMessage = "Mã màu không được vượt quá 20 ký tự")]
        public string? Color { get; set; }

        public bool IsActive { get; set; }
    }

    public class CategoryResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public CategoryDto? Category { get; set; }
    }

    public class CategoriesListResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<CategoryDto> Categories { get; set; } = new();
        public int TotalCount { get; set; }
    }
}