namespace ELibraryManagement.Web.Models
{
    public class AdminBookViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;
        public int PublicationYear { get; set; }
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }
        public int AvailableQuantity { get; set; }
        public int RequestedCount { get; set; }
        public int BorrowedQuantity => TotalQuantity - AvailableQuantity;
        public string Language { get; set; } = string.Empty;
        public int PageCount { get; set; }
        public decimal AverageRating { get; set; }
        public int RatingCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Category information
        public List<CategoryInfo> Categories { get; set; } = new List<CategoryInfo>();
        public string CategoryNames => string.Join(", ", Categories.Select(c => c.Name));

        // Status information
        public string Status => AvailableQuantity > 0 ? "Có sẵn" : "Hết sách";
        public string StatusClass => AvailableQuantity > 0 ? "text-success" : "text-danger";
    }

    public class CategoryInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
    }
}
