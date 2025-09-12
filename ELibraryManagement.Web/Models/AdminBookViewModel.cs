using System.ComponentModel.DataAnnotations;

namespace ELibraryManagement.Web.Models
{
    public class CreateBookViewModel
    {
        [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tiêu đề không được dài quá 200 ký tự")]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tác giả là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tác giả không được dài quá 100 ký tự")]
        [Display(Name = "Tác giả")]
        public string Author { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "ISBN không được dài quá 20 ký tự")]
        [Display(Name = "ISBN")]
        public string? ISBN { get; set; }

        [StringLength(100, ErrorMessage = "Nhà xuất bản không được dài quá 100 ký tự")]
        [Display(Name = "Nhà xuất bản")]
        public string? Publisher { get; set; }

        [Range(1000, 2100, ErrorMessage = "Năm xuất bản phải từ 1000 đến 2100")]
        [Display(Name = "Năm xuất bản")]
        public int PublicationYear { get; set; } = DateTime.Now.Year;

        [StringLength(1000, ErrorMessage = "Mô tả không được dài quá 1000 ký tự")]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Url(ErrorMessage = "URL hình ảnh phải là URL hợp lệ")]
        [Display(Name = "URL hình bìa")]
        public string? CoverImageUrl { get; set; }

        [Required(ErrorMessage = "Số lượng là bắt buộc")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng phải là số không âm")]
        [Display(Name = "Số lượng")]
        public int Quantity { get; set; }

        [StringLength(50, ErrorMessage = "Ngôn ngữ không được dài quá 50 ký tự")]
        [Display(Name = "Ngôn ngữ")]
        public string? Language { get; set; } = "Tiếng Việt";

        [Range(0, int.MaxValue, ErrorMessage = "Số trang phải là số không âm")]
        [Display(Name = "Số trang")]
        public int PageCount { get; set; }

        [Display(Name = "Danh mục")]
        public List<int>? CategoryIds { get; set; }
    }

    public class UpdateBookViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tiêu đề không được dài quá 200 ký tự")]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tác giả là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tác giả không được dài quá 100 ký tự")]
        [Display(Name = "Tác giả")]
        public string Author { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "ISBN không được dài quá 20 ký tự")]
        [Display(Name = "ISBN")]
        public string? ISBN { get; set; }

        [StringLength(100, ErrorMessage = "Nhà xuất bản không được dài quá 100 ký tự")]
        [Display(Name = "Nhà xuất bản")]
        public string? Publisher { get; set; }

        [Range(1000, 2100, ErrorMessage = "Năm xuất bản phải từ 1000 đến 2100")]
        [Display(Name = "Năm xuất bản")]
        public int PublicationYear { get; set; }

        [StringLength(1000, ErrorMessage = "Mô tả không được dài quá 1000 ký tự")]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Url(ErrorMessage = "URL hình ảnh phải là URL hợp lệ")]
        [Display(Name = "URL hình bìa")]
        public string? CoverImageUrl { get; set; }

        [Required(ErrorMessage = "Số lượng là bắt buộc")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng phải là số không âm")]
        [Display(Name = "Số lượng")]
        public int Quantity { get; set; }

        [StringLength(50, ErrorMessage = "Ngôn ngữ không được dài quá 50 ký tự")]
        [Display(Name = "Ngôn ngữ")]
        public string? Language { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Số trang phải là số không âm")]
        [Display(Name = "Số trang")]
        public int PageCount { get; set; }

        [Display(Name = "Danh mục")]
        public List<int>? CategoryIds { get; set; }
    }

    public class CategoryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Color { get; set; }
    }
}