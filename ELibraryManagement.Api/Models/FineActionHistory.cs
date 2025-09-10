using System.ComponentModel.DataAnnotations;

namespace ELibraryManagement.Api.Models
{
    public class FineActionHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int FineId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty; // Người thực hiện hành động

        [Required]
        public FineActionType ActionType { get; set; }

        [Required]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        // Thông tin bổ sung
        public decimal? Amount { get; set; } // Số tiền liên quan (nếu có)
        public string? Notes { get; set; } // Ghi chú chi tiết

        // Timestamp
        public DateTime ActionDate { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Fine Fine { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;
    }

    public enum FineActionType
    {
        ReminderSent,      // Gửi nhắc nhở
        PaymentReceived,   // Nhận thanh toán
        Escalated,         // Chuyển lên cấp trên
        AccountSuspended,  // Tạm ngừng tài khoản
        AccountBlocked,    // Chặn tài khoản
        FineWaived,        // Miễn phạt
        FineWrittenOff,    // Xóa nợ
        LegalAction,       // Hành động pháp lý
        Settlement         // Hòa giải
    }
}
