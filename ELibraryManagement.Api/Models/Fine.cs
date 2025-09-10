using System.ComponentModel.DataAnnotations;

namespace ELibraryManagement.Api.Models
{
    public class Fine
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public int? BorrowRecordId { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(100)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public FineStatus Status { get; set; } = FineStatus.Pending;

        public DateTime FineDate { get; set; } = DateTime.UtcNow;

        public DateTime? PaidDate { get; set; }

        // Theo dõi trường hợp trốn phạt
        public DateTime? DueDate { get; set; } // Hạn thanh toán phạt
        public int ReminderCount { get; set; } = 0; // Số lần đã nhắc nhở
        public DateTime? LastReminderDate { get; set; } // Lần nhắc nhở cuối
        public string? EscalationReason { get; set; } // Lý do chuyển lên cấp trên
        public DateTime? EscalationDate { get; set; } // Ngày chuyển lên cấp trên

        // Timestamp
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual BorrowRecord? BorrowRecord { get; set; }
    }

    public enum FineStatus
    {
        Pending,
        Paid,
        Waived,
        Overdue,
        Escalated,
        WrittenOff
    }
}
