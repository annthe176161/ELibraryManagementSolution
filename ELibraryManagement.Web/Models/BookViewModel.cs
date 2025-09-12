namespace ELibraryManagement.Web.Models
{
    public class BookViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;
        public int PublicationYear { get; set; }
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public int PageCount { get; set; }
        public decimal AverageRating { get; set; }
        public int RatingCount { get; set; }
    }
}
