using System.ComponentModel.DataAnnotations;

namespace ELibraryManagement.Api.Models
{
    public class UserStatus
    {
        [Key]
        public string UserId { get; set; } = string.Empty;

        // Trạng thái tài khoản
        public UserAccountStatus AccountStatus { get; set; } = UserAccountStatus.Active;

        // Thông tin phạt chưa trả
        public decimal TotalOutstandingFines { get; set; } = 0;
        public int OverdueFinesCount { get; set; } = 0;

        // Giới hạn mượn sách
        public int MaxBorrowLimit { get; set; } = 5; // Mặc định 5 cuốn
        public int CurrentBorrowCount { get; set; } = 0;

        // Lý do chặn (nếu có)
        public string? BlockReason { get; set; }
        public DateTime? BlockedUntil { get; set; }

        // Timestamp
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
    }

    public enum UserAccountStatus
    {
        Active,
        Suspended,
        Blocked,
        Probation
    }
}
