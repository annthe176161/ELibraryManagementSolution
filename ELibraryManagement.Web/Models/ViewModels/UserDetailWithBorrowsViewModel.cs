namespace ELibraryManagement.Web.Models
{
    public class UserDetailWithBorrowsViewModel
    {
        public AdminUserViewModel? User { get; set; }
        public List<UserBorrowedBookViewModel> BorrowedBooks { get; set; } = new List<UserBorrowedBookViewModel>();
    }
}
