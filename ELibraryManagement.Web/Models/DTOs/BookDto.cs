namespace ELibraryManagement.Web.Models.DTOs
{
    public class BookDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string? ISBN { get; set; }
        public string? Publisher { get; set; }
        public int PublicationYear { get; set; }
        public string? Description { get; set; }
        public string? CoverImageUrl { get; set; }
        public int Quantity { get; set; }
        public int AvailableQuantity { get; set; }
        public int RequestedCount { get; set; }
        public string? Language { get; set; }
        public int PageCount { get; set; }
        public float AverageRating { get; set; }
        public int RatingCount { get; set; }
        public List<CategoryDto>? Categories { get; set; }
    }

    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Color { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int BookCount { get; set; }
    }

    public class CategoriesListResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<CategoryDto> Categories { get; set; } = new();
        public int TotalCount { get; set; }
    }
}