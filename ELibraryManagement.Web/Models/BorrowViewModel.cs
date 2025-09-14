namespace ELibraryManagement.Web.Models
{
    public class ExtendBorrowResponseViewModel
    {
        public bool Success { get; set; }
        public int BorrowRecordId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public DateTime OldDueDate { get; set; }
        public DateTime NewDueDate { get; set; }
        public int ExtensionCount { get; set; }
        public int RemainingExtensions => Math.Max(0, 2 - ExtensionCount);
        public string Message { get; set; } = string.Empty;
    }

    public class ExtendBorrowRequestViewModel
    {
        public string? Reason { get; set; }
    }

    public class BorrowConfirmationViewModel
    {
        public BookViewModel Book { get; set; } = new();
        public StudentInfoViewModel Student { get; set; } = new();
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public int MaxExtensions { get; set; }
        public int BorrowDurationDays => (DueDate - BorrowDate).Days;
    }

    public class StudentInfoViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Faculty { get; set; } = string.Empty;
        public string Class { get; set; } = string.Empty;
        public string Major { get; set; } = string.Empty;
        public string AcademicYear { get; set; } = string.Empty;
        public string StudentStatus { get; set; } = string.Empty;
    }

    public class BorrowResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? BorrowRecordId { get; set; }
    }
}