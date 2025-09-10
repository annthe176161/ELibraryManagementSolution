using System.ComponentModel.DataAnnotations;

namespace ELibraryManagement.Api.Models
{
    public class BorrowRecord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public int BookId { get; set; }

        public DateTime BorrowDate { get; set; } = DateTime.UtcNow;

        public DateTime DueDate { get; set; }

        public DateTime? ReturnDate { get; set; }

        public BorrowStatus Status { get; set; } = BorrowStatus.Borrowed;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public decimal? RentalPrice { get; set; } // Giá thuê tại thời điểm mượn

        public bool IsOverdue => ReturnDate == null && DateTime.UtcNow > DueDate;

        public int OverdueDays => IsOverdue ? (DateTime.UtcNow - DueDate).Days : 0;

        // Timestamp
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual Book Book { get; set; } = null!;
        public virtual ICollection<Fine> Fines { get; set; } = new List<Fine>();
    }

    public enum BorrowStatus
    {
        Borrowed,
        Returned,
        Lost,
        Damaged
    }
}
