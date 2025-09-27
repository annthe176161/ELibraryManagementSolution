namespace ELibraryManagement.Web.Models
{
    public class ReturnBookResponseViewModel
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? BorrowRecordId { get; set; }
        public decimal? FineAmount { get; set; }
        public DateTime? ReturnDate { get; set; }
    }
}
