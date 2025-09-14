using System.ComponentModel.DataAnnotations;

namespace ELibraryManagement.Web.Models
{
    public class BorrowBookViewModel
    {
        [Required]
        public int BookId { get; set; }

        public string BookTitle { get; set; } = string.Empty;
        public string BookAuthor { get; set; } = string.Empty;
        public string BookCoverUrl { get; set; } = string.Empty;

        [Display(Name = "Ngày trả dự kiến")]
        [DataType(DataType.Date)]
        public DateTime? DueDate { get; set; }

        [Display(Name = "Ghi chú")]
        [MaxLength(500, ErrorMessage = "Ghi chú không được quá 500 ký tự")]
        public string? Notes { get; set; }

        // Student Information
        public StudentInfoViewModel? StudentInfo { get; set; }
    }

    public class BorrowBookRequestViewModel
    {
        [Required]
        public int BookId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public DateTime? DueDate { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class BorrowBookResponseViewModel
    {
        public int BorrowRecordId { get; set; }
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool Success { get; set; }
    }

    public class UserBorrowedBookViewModel
    {
        public int BorrowRecordId { get; set; }
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string BookAuthor { get; set; } = string.Empty;
        public string BookCoverUrl { get; set; } = string.Empty;
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public bool IsOverdue => DueDate < DateTime.Now && ReturnDate == null;
        public int DaysOverdue => IsOverdue ? (DateTime.Now - DueDate).Days : 0;
    }
}
