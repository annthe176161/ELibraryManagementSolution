namespace ELibraryManagement.Api.Models
{
    public class BookCategory
    {
        public int BookId { get; set; }
        public int CategoryId { get; set; }

        // Navigation properties
        public virtual Book Book { get; set; } = null!;
        public virtual Category Category { get; set; } = null!;
    }
}
