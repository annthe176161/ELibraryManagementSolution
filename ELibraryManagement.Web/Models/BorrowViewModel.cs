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
}