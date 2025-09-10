using System.ComponentModel.DataAnnotations;

namespace ELibraryManagement.Api.Models
{
    public class Book
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Author { get; set; } = string.Empty;

        [MaxLength(13)]
        public string? ISBN { get; set; }

        [MaxLength(100)]
        public string? Publisher { get; set; }

        public int PublicationYear { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? CoverImageUrl { get; set; }

        public int Quantity { get; set; } = 1;

        public int AvailableQuantity { get; set; } = 1;

        public bool IsAvailable => AvailableQuantity > 0;

        public decimal? Price { get; set; }

        [MaxLength(50)]
        public string? Language { get; set; } = "Vietnamese";

        public int PageCount { get; set; }

        // Rating calculation
        public double AverageRating { get; set; } = 0;
        public int RatingCount { get; set; } = 0;

        // Timestamp và soft delete
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        public virtual ICollection<BookCategory> BookCategories { get; set; } = new List<BookCategory>();
        public virtual ICollection<BorrowRecord> BorrowRecords { get; set; } = new List<BorrowRecord>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
