using System.ComponentModel.DataAnnotations;

namespace ELibraryManagement.Api.DTOs
{
    public class BorrowBookRequestDto
    {
        [Required]
        public int BookId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public DateTime? DueDate { get; set; } // Optional, if not provided, use default (e.g., 14 days)

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class BorrowBookResponseDto
    {
        public int BorrowRecordId { get; set; }
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal? RentalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class BorrowRecordDto
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string BookAuthor { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal? RentalPrice { get; set; }
        public string? Notes { get; set; }
        public bool IsOverdue => ReturnDate == null && DateTime.UtcNow > DueDate;
        public int OverdueDays => IsOverdue ? (DateTime.UtcNow - DueDate).Days : 0;
    }

    public class ReturnBookResponseDto
    {
        public bool Success { get; set; }
        public int BorrowRecordId { get; set; }
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public DateTime ReturnDate { get; set; }
        public decimal? FineAmount { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
