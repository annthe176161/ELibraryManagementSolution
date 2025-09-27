using System.ComponentModel.DataAnnotations;

namespace ELibraryManagement.Web.Models.DTOs
{
    public class CreateBookDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tác giả là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tác giả không được vượt quá 100 ký tự")]
        public string Author { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "ISBN không được vượt quá 20 ký tự")]
        public string? ISBN { get; set; }

        [StringLength(100, ErrorMessage = "Nhà xuất bản không được vượt quá 100 ký tự")]
        public string? Publisher { get; set; }

        [Range(1000, 2100, ErrorMessage = "Năm xuất bản phải từ 1000 đến 2100")]
        public int PublicationYear { get; set; }

        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Số lượng là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int TotalQuantity { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Số trang phải >= 0")]
        public int PageCount { get; set; }

        [StringLength(50, ErrorMessage = "Ngôn ngữ không được vượt quá 50 ký tự")]
        public string? Language { get; set; }

        public List<int> CategoryIds { get; set; } = new List<int>();
    }
}
