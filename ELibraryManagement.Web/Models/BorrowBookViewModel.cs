using System.ComponentModel.DataAnnotations;
using ELibraryManagement.Web.Helpers;

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
        // ConfirmedDate is stored in the API as UTC. We keep the UTC value here and
        // convert to Vietnam time in the views when displaying.
        public DateTime? ConfirmedDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public decimal? FineAmount { get; set; }
        public string? FineStatus { get; set; }
        public string? FineReason { get; set; }
        // Helper that returns the UTC DateTime to use as borrow start: prefer ConfirmedDate when present
        public DateTime DisplayBorrowDateUtc => ConfirmedDate ?? BorrowDate;
        public bool IsOverdue => DueDate < DateTimeHelper.VietnamNow() && ReturnDate == null;
        public int DaysOverdue => IsOverdue ? (DateTimeHelper.VietnamNow() - DueDate).Days : 0;
        public bool HasFine => FineAmount.HasValue && FineAmount.Value > 0;
        public string FineAmountFormatted => FineAmount.HasValue ? FineAmount.Value.ToString("N0") + " VND" : "Không có phạt";
    }
}
