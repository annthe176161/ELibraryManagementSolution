using ELibraryManagement.Web.Models.DTOs.CategoryDtos;
using System.ComponentModel.DataAnnotations;

namespace ELibraryManagement.Web.Models.ViewModels
{
    public class CategoryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Color { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int BookCount { get; set; }
        public string StatusText => IsActive ? "Hoạt động" : "Không hoạt động";
        public string StatusBadgeClass => IsActive ? "bg-success" : "bg-secondary";
    }

    public class CreateCategoryViewModel
    {
        [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên danh mục không được vượt quá 100 ký tự")]
        [Display(Name = "Tên danh mục")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [StringLength(20, ErrorMessage = "Mã màu không được vượt quá 20 ký tự")]
        [Display(Name = "Màu sắc")]
        public string? Color { get; set; }

        [Display(Name = "Trạng thái hoạt động")]
        public bool IsActive { get; set; } = true;

        public CreateCategoryDto ToDto()
        {
            return new CreateCategoryDto
            {
                Name = Name,
                Description = Description,
                Color = Color,
                IsActive = IsActive
            };
        }
    }

    public class EditCategoryViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên danh mục không được vượt quá 100 ký tự")]
        [Display(Name = "Tên danh mục")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [StringLength(20, ErrorMessage = "Mã màu không được vượt quá 20 ký tự")]
        [Display(Name = "Màu sắc")]
        public string? Color { get; set; }

        [Display(Name = "Trạng thái hoạt động")]
        public bool IsActive { get; set; }

        public static EditCategoryViewModel FromDto(CategoryDto dto)
        {
            return new EditCategoryViewModel
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                Color = dto.Color,
                IsActive = dto.IsActive
            };
        }

        public UpdateCategoryDto ToDto()
        {
            return new UpdateCategoryDto
            {
                Name = Name,
                Description = Description,
                Color = Color,
                IsActive = IsActive
            };
        }
    }

    public class UpdateCategoryViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên danh mục không được vượt quá 100 ký tự")]
        [Display(Name = "Tên danh mục")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [StringLength(20, ErrorMessage = "Mã màu không được vượt quá 20 ký tự")]
        [Display(Name = "Màu sắc")]
        public string? Color { get; set; }

        [Display(Name = "Trạng thái hoạt động")]
        public bool IsActive { get; set; }

        public UpdateCategoryDto ToDto()
        {
            return new UpdateCategoryDto
            {
                Name = Name,
                Description = Description,
                Color = Color,
                IsActive = IsActive
            };
        }
    }

    public class CategoryListViewModel
    {
        public List<CategoryViewModel> Categories { get; set; } = new();
        public int TotalCount { get; set; }
        public bool IncludeInactive { get; set; }
        public string SearchTerm { get; set; } = string.Empty;

        public static CategoryListViewModel FromDto(CategoriesListResponseDto dto)
        {
            return new CategoryListViewModel
            {
                Categories = dto.Categories.Select(c => new CategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Color = c.Color,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    BookCount = c.BookCount
                }).ToList(),
                TotalCount = dto.TotalCount
            };
        }
    }

    public class CategoryDeleteViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int BookCount { get; set; }
        public bool CanDelete => BookCount == 0;
        public string DeleteWarning => BookCount > 0
            ? $"Không thể xóa danh mục này vì có {BookCount} sách đang sử dụng."
            : "Bạn có chắc chắn muốn xóa danh mục này?";

        public static CategoryDeleteViewModel FromDto(CategoryDto dto)
        {
            return new CategoryDeleteViewModel
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                BookCount = dto.BookCount
            };
        }
    }
}