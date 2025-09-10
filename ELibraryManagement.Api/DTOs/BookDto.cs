using System.ComponentModel.DataAnnotations;

namespace ELibraryManagement.Api.DTOs
{
    public class BookDto
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Author { get; set; } = string.Empty;

        public string? ISBN { get; set; }

        public string? Publisher { get; set; }

        public int PublicationYear { get; set; }

        public string? Description { get; set; }

        public string? CoverImageUrl { get; set; }

        public int Quantity { get; set; }

        public int AvailableQuantity { get; set; }

        public decimal? Price { get; set; }

        public string? Language { get; set; }

        public int PageCount { get; set; }

        public float AverageRating { get; set; }

        public int RatingCount { get; set; }

        // Navigation properties (optional for DTO)
        public List<CategoryDto>? Categories { get; set; }
    }

    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Color { get; set; }
    }
}
