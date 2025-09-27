using System.ComponentModel.DataAnnotations;

namespace ELibraryManagement.Web.Models
{
    public class CreateFineViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn sinh viên")]
        [Display(Name = "Sinh viên")]
        public string UserId { get; set; } = string.Empty;

        [Display(Name = "Bản ghi mượn sách")]
        public int? BorrowRecordId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số tiền phạt")]
        [Range(1000, 10000000, ErrorMessage = "Số tiền phạt phải từ 1,000 đến 10,000,000 VND")]
        [Display(Name = "Số tiền phạt (VND)")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lý do phạt")]
        [StringLength(100, ErrorMessage = "Lý do phạt không được vượt quá 100 ký tự")]
        [Display(Name = "Lý do phạt")]
        public string Reason { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        [Display(Name = "Mô tả chi tiết")]
        public string? Description { get; set; }

        [Display(Name = "Hạn thanh toán")]
        [DataType(DataType.Date)]
        public DateTime? DueDate { get; set; } = DateTime.Now.AddDays(30);
    }

    public class EditFineViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số tiền phạt")]
        [Range(1000, 10000000, ErrorMessage = "Số tiền phạt phải từ 1,000 đến 10,000,000 VND")]
        [Display(Name = "Số tiền phạt (VND)")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lý do phạt")]
        [StringLength(100, ErrorMessage = "Lý do phạt không được vượt quá 100 ký tự")]
        [Display(Name = "Lý do phạt")]
        public string Reason { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        [Display(Name = "Mô tả chi tiết")]
        public string? Description { get; set; }

        [Display(Name = "Hạn thanh toán")]
        [DataType(DataType.Date)]
        public DateTime? DueDate { get; set; }

        [Display(Name = "Trạng thái")]
        public string? Status { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        [Display(Name = "Ghi chú")]
        public string? Notes { get; set; }
    }

    public class PayFineViewModel
    {
        public int Id { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Reason { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        [Display(Name = "Ghi chú thanh toán")]
        public string? Notes { get; set; }
    }

    public class WaiveFineViewModel
    {
        public int Id { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string OriginalReason { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập lý do miễn phạt")]
        [StringLength(200, ErrorMessage = "Lý do miễn phạt không được vượt quá 200 ký tự")]
        [Display(Name = "Lý do miễn phạt")]
        public string Reason { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        [Display(Name = "Ghi chú")]
        public string? Notes { get; set; }
    }

    public class FineFilterViewModel
    {
        [Display(Name = "Trạng thái")]
        public string? Status { get; set; }

        [Display(Name = "Tìm kiếm")]
        public string? Search { get; set; }

        [Display(Name = "Từ ngày")]
        [DataType(DataType.Date)]
        public DateTime? FromDate { get; set; }

        [Display(Name = "Đến ngày")]
        [DataType(DataType.Date)]
        public DateTime? ToDate { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public static class FineStatuses
    {
        public const string Pending = "Pending";
        public const string Paid = "Paid";
        public const string Waived = "Waived";
        public const string Overdue = "Overdue";
        public const string Escalated = "Escalated";
        public const string WrittenOff = "WrittenOff";

        public static Dictionary<string, string> GetDisplayNames()
        {
            return new Dictionary<string, string>
            {
                { Pending, "Chờ thanh toán" },
                { Paid, "Đã thanh toán" },
                { Waived, "Đã miễn" },
                { Overdue, "Quá hạn" },
                { Escalated, "Đã chuyển lên" },
                { WrittenOff, "Đã xóa nợ" }
            };
        }

        public static string GetDisplayName(string status)
        {
            var displayNames = GetDisplayNames();
            return displayNames.ContainsKey(status) ? displayNames[status] : status;
        }
    }

    public static class FineActionTypes
    {
        public const string ReminderSent = "ReminderSent";
        public const string PaymentReceived = "PaymentReceived";
        public const string Escalated = "Escalated";
        public const string AccountSuspended = "AccountSuspended";
        public const string AccountBlocked = "AccountBlocked";
        public const string FineWaived = "FineWaived";
        public const string FineWrittenOff = "FineWrittenOff";
        public const string LegalAction = "LegalAction";
        public const string Settlement = "Settlement";

        public static Dictionary<string, string> GetDisplayNames()
        {
            return new Dictionary<string, string>
            {
                { ReminderSent, "Gửi nhắc nhở" },
                { PaymentReceived, "Nhận thanh toán" },
                { Escalated, "Chuyển lên cấp trên" },
                { AccountSuspended, "Tạm ngừng tài khoản" },
                { AccountBlocked, "Chặn tài khoản" },
                { FineWaived, "Miễn phạt" },
                { FineWrittenOff, "Xóa nợ" },
                { LegalAction, "Hành động pháp lý" },
                { Settlement, "Hòa giải" }
            };
        }

        public static string GetDisplayName(string actionType)
        {
            var displayNames = GetDisplayNames();
            return displayNames.ContainsKey(actionType) ? displayNames[actionType] : actionType;
        }
    }

    // API Service ViewModels
    public class FineViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserFullName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public int? BorrowRecordId { get; set; }
        public string? BookTitle { get; set; }
        public decimal Amount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime FineDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public DateTime? DueDate { get; set; }
        public int ReminderCount { get; set; }
        public DateTime? LastReminderDate { get; set; }
        public string? EscalationReason { get; set; }
        public DateTime? EscalationDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsOverdue { get; set; }
    }

    public class FineDetailViewModel : FineViewModel
    {
        public string? BookAuthor { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<FineActionHistoryViewModel> ActionHistory { get; set; } = new List<FineActionHistoryViewModel>();
    }

    public class FineActionHistoryViewModel
    {
        public int Id { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal? Amount { get; set; }
        public string? Notes { get; set; }
        public DateTime ActionDate { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
    }

    public class FineStatisticsViewModel
    {
        public int TotalFines { get; set; }
        public int PendingFines { get; set; }
        public int PaidFines { get; set; }
        public int WaivedFines { get; set; }
        public int OverdueFines { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal PendingAmount { get; set; }
    }

    public class CreateFineRequest
    {
        public string UserId { get; set; } = string.Empty;
        public int? BorrowRecordId { get; set; }
        public decimal Amount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public string? FineType { get; set; } // overdue, lost, damaged, other
    }

    public class UpdateFineRequest
    {
        public decimal Amount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public string? Status { get; set; }
        public string? Notes { get; set; }
    }
}
