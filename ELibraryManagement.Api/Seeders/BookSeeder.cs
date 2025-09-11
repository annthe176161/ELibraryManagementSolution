using ELibraryManagement.Api.Data;
using ELibraryManagement.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ELibraryManagement.Api.Seeders
{
    public class BookSeeder
    {
        public static void SeedBooks(ModelBuilder modelBuilder)
        {
            // This is an example of how you could seed additional books
            // The main seeding is done in SeedData.cs in the Data folder

            // You can add more seed data here if needed
            // For example:
            /*
            modelBuilder.Entity<Book>().HasData(
                new Book
                {
                    Id = 100,
                    Title = "Additional Seeded Book",
                    Author = "Seeder Author",
                    ISBN = "9999999999",
                    Publisher = "Seeder Publisher",
                    PublicationYear = 2024,
                    Description = "A book seeded by BookSeeder",
                    CoverImageUrl = "/images/seeded-book.jpg",
                    Quantity = 5,
                    AvailableQuantity = 5,
                    Price = 50000,
                    Language = "Tiếng Việt",
                    PageCount = 200,
                    AverageRating = 4.0f,
                    RatingCount = 10,
                    CreatedAt = DateTime.UtcNow
                }
            );
            */
        }
    }
}
