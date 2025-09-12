namespace ELibraryManagement.Web.Models
{
    public class BorrowedBookDetailViewModel
    {
        public UserBorrowedBookViewModel BorrowRecord { get; set; } = new();
        public BookViewModel BookDetails { get; set; } = new();
    }
}