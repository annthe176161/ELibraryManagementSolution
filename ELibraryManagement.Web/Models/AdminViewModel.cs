namespace ELibraryManagement.Web.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalBooks { get; set; }
        public int TotalBorrows { get; set; }
        public int TotalReviews { get; set; }
        public int ActiveBorrows { get; set; }
        public int OverdueBorrows { get; set; }
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
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string Status { get; set; } = "";
        public string? Notes { get; set; }
        public decimal? FineAmount { get; set; }
        public bool IsOverdue => ReturnDate == null && DateTime.Now > DueDate;
        public int OverdueDays => IsOverdue ? (DateTime.Now - DueDate).Days : 0;
        public string StatusDisplay => Status switch
        {
            "Borrowed" => "Đang mượn",
            "Returned" => "Đã trả",
            "Overdue" => "Quá hạn",
            _ => Status
        };
        public string StatusClass => Status switch
        {
            "Borrowed" => "success",
            "Returned" => "secondary",
            "Overdue" => "danger",
            _ => "primary"
        };
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
    }
}