using ELibraryManagement.Web.Helpers;

namespace ELibraryManagement.Web.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalStudents { get; set; } // Chỉ sinh viên, không tính admin
        public int TotalBooks { get; set; }
        public int TotalBorrows { get; set; }
        public int ActiveBorrows { get; set; } // Sách đang được mượn thực sự
        public int RequestedBorrows { get; set; } // Sách đã đăng ký mượn
        public int ReturnedBorrows { get; set; } // Sách đã trả
        public int CancelledBorrows { get; set; } // Sách đã hủy
        public int OverdueBorrows { get; set; } // Sách quá hạn
        public int LostBorrows { get; set; } // Sách mất
        public int DamagedBorrows { get; set; } // Sách hư hỏng
        public int TotalReviews { get; set; }
        public List<RecentActivityViewModel> RecentActivities { get; set; } = new();
    }

    public class RecentActivityViewModel
    {
        public string Activity { get; set; } = "";
        public string UserName { get; set; } = "";
        public string Details { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }

    public class BorrowRecordViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; } = "";
        public string UserName { get; set; } = "";
        public string UserEmail { get; set; } = "";
        public int BookId { get; set; }
        public string BookTitle { get; set; } = "";
        public string BookAuthor { get; set; } = "";
        public string BookCoverUrl { get; set; } = "";
        public DateTime BorrowDate { get; set; }
        public DateTime? ConfirmedDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string Status { get; set; } = "";
        public string? Notes { get; set; }
        public decimal? FineAmount { get; set; }
        public bool IsOverdue => ReturnDate == null && DateTimeHelper.VietnamNow() > DueDate.ToVietnamTime();
        public int OverdueDays => IsOverdue ? (DateTimeHelper.VietnamNow().Date - DueDate.ToVietnamTime().Date).Days : 0;
        public string StatusDisplay => Status switch
        {
            "Requested" => "Đã đăng ký",
            "Borrowed" => "Đang mượn",
            "Returned" => "Đã trả",
            "Cancelled" => "Đã hủy",
            "Lost" => "Mất sách",
            "Damaged" => "Hư hỏng",
            "Overdue" => "Quá hạn",
            _ => Status
        };
        public string StatusClass => Status switch
        {
            "Requested" => "warning",
            "Borrowed" => "success",
            "Returned" => "secondary",
            "Cancelled" => "danger",
            "Lost" => "dark",
            "Damaged" => "dark",
            "Overdue" => "danger",
            _ => "primary"
        };
    }

    public class BorrowDetailViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; } = "";
        public string UserName { get; set; } = "";
        public string UserEmail { get; set; } = "";
        public string StudentId { get; set; } = "";
        public string UserPhoneNumber { get; set; } = "";
        public int BookId { get; set; }
        public string BookTitle { get; set; } = "";
        public string BookAuthor { get; set; } = "";
        public string BookCoverUrl { get; set; } = "";
        public string BookIsbn { get; set; } = "";
        public DateTime BorrowDate { get; set; }
        public DateTime? ConfirmedDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string Status { get; set; } = "";
        public string? Notes { get; set; }
        public decimal? FineAmount { get; set; }
        public bool IsOverdue => ReturnDate == null && DateTimeHelper.VietnamNow() > DueDate.ToVietnamTime();
        public int OverdueDays => IsOverdue ? (DateTimeHelper.VietnamNow().Date - DueDate.ToVietnamTime().Date).Days : 0;
        public string StatusDisplay => Status switch
        {
            "Requested" => "Đã đăng ký",
            "Borrowed" => "Đang mượn",
            "Returned" => "Đã trả",
            "Cancelled" => "Đã hủy",
            "Lost" => "Mất sách",
            "Damaged" => "Hư hỏng",
            "Overdue" => "Quá hạn",
            _ => Status
        };
        public string StatusClass => Status switch
        {
            "Requested" => "warning",
            "Borrowed" => "success",
            "Returned" => "secondary",
            "Cancelled" => "danger",
            "Lost" => "dark",
            "Damaged" => "dark",
            "Overdue" => "danger",
            _ => "primary"
        };
        public string FormattedBorrowDate => BorrowDate.ToString("dd/MM/yyyy HH:mm");
        public string FormattedDueDate => DueDate.ToString("dd/MM/yyyy");
        public string FormattedReturnDate => ReturnDate?.ToString("dd/MM/yyyy HH:mm") ?? "";
    }

    public class AdminUserViewModel
    {
        public string Id { get; set; } = "";
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string FullName => $"{FirstName} {LastName}".Trim();
        public string StudentId { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string Address { get; set; } = "";
        public DateTime? DateOfBirth { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> Roles { get; set; } = new();
        public int TotalBorrows { get; set; }
        public int ActiveBorrows { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? LastLoginDate { get; set; }
    }
}
