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

        public BorrowStatus Status { get; set; } = BorrowStatus.Requested;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public int ExtensionCount { get; set; } = 0; // Số lần đã gia hạn

        public DateTime? LastExtensionDate { get; set; } // Ngày gia hạn gần nhất

        public DateTime? ConfirmedDate { get; set; } // Ngày admin xác nhận mượn

        public bool IsOverdue => Status == BorrowStatus.Borrowed && ReturnDate == null && DateTime.UtcNow > DueDate;

        public int OverdueDays => IsOverdue ? (DateTime.UtcNow - DueDate).Days : 0;

        public bool CanExtend => false; // Chức năng gia hạn đã bị vô hiệu hóa

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
        Requested,  // Sinh viên yêu cầu mượn (chờ xác nhận)
        Borrowed,   // Đang mượn (đã xác nhận)
        Returned,   // Đã trả
        Lost,       // Mất sách
        Damaged,    // Hư hỏng
        Cancelled   // Hủy yêu cầu
    }
}
