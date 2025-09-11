using ELibraryManagement.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ELibraryManagement.Api.Data
{
    public static class SeedData
    {
        public static void Initialize(ModelBuilder modelBuilder)
        {
            // Seed Roles
            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole { Id = "1", Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole { Id = "2", Name = "Librarian", NormalizedName = "LIBRARIAN" },
                new IdentityRole { Id = "3", Name = "User", NormalizedName = "USER" }
            );

            // Seed Categories - Danh mục sách tiếng Việt
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Tiểu Thuyết", Description = "Các tác phẩm tiểu thuyết, truyện dài", Color = "#FF6B6B", CreatedAt = DateTime.UtcNow },
                new Category { Id = 2, Name = "Phi Tiểu Thuyết", Description = "Sách phi hư cấu, sách thực tế", Color = "#4ECDC4", CreatedAt = DateTime.UtcNow },
                new Category { Id = 3, Name = "Khoa Học", Description = "Sách khoa học và công nghệ", Color = "#45B7D1", CreatedAt = DateTime.UtcNow },
                new Category { Id = 4, Name = "Lịch Sử", Description = "Sách lịch sử và văn hóa", Color = "#96CEB4", CreatedAt = DateTime.UtcNow },
                new Category { Id = 5, Name = "Tiểu Sử", Description = "Tiểu sử và hồi ký", Color = "#FECA57", CreatedAt = DateTime.UtcNow },
                new Category { Id = 6, Name = "Lập Trình", Description = "Sách lập trình và phát triển phần mềm", Color = "#FF9FF3", CreatedAt = DateTime.UtcNow },
                new Category { Id = 7, Name = "Kinh Doanh", Description = "Sách kinh doanh và kinh tế", Color = "#54A0FF", CreatedAt = DateTime.UtcNow },
                new Category { Id = 8, Name = "Văn Học Việt Nam", Description = "Tác phẩm văn học Việt Nam", Color = "#FF8A65", CreatedAt = DateTime.UtcNow },
                new Category { Id = 9, Name = "Thiếu Nhi", Description = "Sách dành cho thiếu nhi", Color = "#81C784", CreatedAt = DateTime.UtcNow },
                new Category { Id = 10, Name = "Nấu Ăn", Description = "Sách công thức nấu ăn", Color = "#FFB74D", CreatedAt = DateTime.UtcNow }
            );

            // Seed Books - Sách mẫu tiếng Việt
            modelBuilder.Entity<Book>().HasData(
                new Book
                {
                    Id = 1,
                    Title = "Đắc Nhân Tâm",
                    Author = "Dale Carnegie",
                    ISBN = "9786047770560",
                    Publisher = "Nhà Xuất Bản Tổng Hợp TP.HCM",
                    PublicationYear = 2020,
                    Description = "Cuốn sách về nghệ thuật giao tiếp và thu phục lòng người",
                    CoverImageUrl = "/images/dac-nhan-tam.jpg",
                    Quantity = 10,
                    AvailableQuantity = 8,
                    Price = 89000,
                    Language = "Tiếng Việt",
                    PageCount = 320,
                    AverageRating = 4.5f,
                    RatingCount = 25,
                    CreatedAt = DateTime.UtcNow
                },
                new Book
                {
                    Id = 2,
                    Title = "Tôi Tài Giỏi, Bạn Cũng Thế",
                    Author = "Adam Khoo",
                    ISBN = "9786047770577",
                    Publisher = "Nhà Xuất Bản Trẻ",
                    PublicationYear = 2019,
                    Description = "Sách về phát triển bản thân và thành công",
                    CoverImageUrl = "/images/toi-tai-gioi.jpg",
                    Quantity = 15,
                    AvailableQuantity = 12,
                    Price = 95000,
                    Language = "Tiếng Việt",
                    PageCount = 280,
                    AverageRating = 4.3f,
                    RatingCount = 18,
                    CreatedAt = DateTime.UtcNow
                },
                new Book
                {
                    Id = 3,
                    Title = "Tắt Đèn",
                    Author = "Ngô Tất Tố",
                    ISBN = "9786047770584",
                    Publisher = "Nhà Xuất Bản Giáo Dục",
                    PublicationYear = 2018,
                    Description = "Tác phẩm kinh điển của văn học Việt Nam",
                    CoverImageUrl = "/images/tat-den.jpg",
                    Quantity = 8,
                    AvailableQuantity = 6,
                    Price = 45000,
                    Language = "Tiếng Việt",
                    PageCount = 200,
                    AverageRating = 4.7f,
                    RatingCount = 32,
                    CreatedAt = DateTime.UtcNow
                },
                new Book
                {
                    Id = 4,
                    Title = "Lập Trình C# Cơ Bản",
                    Author = "Nguyễn Văn A",
                    ISBN = "9786047770591",
                    Publisher = "Nhà Xuất Bản Bách Khoa",
                    PublicationYear = 2021,
                    Description = "Sách hướng dẫn lập trình C# từ cơ bản đến nâng cao",
                    CoverImageUrl = "/images/csharp-basic.jpg",
                    Quantity = 12,
                    AvailableQuantity = 10,
                    Price = 120000,
                    Language = "Tiếng Việt",
                    PageCount = 400,
                    AverageRating = 4.2f,
                    RatingCount = 15,
                    CreatedAt = DateTime.UtcNow
                },
                new Book
                {
                    Id = 5,
                    Title = "Bí Quyết Thành Công Trong Kinh Doanh",
                    Author = "Trần Thị B",
                    ISBN = "9786047770607",
                    Publisher = "Nhà Xuất Bản Lao Động",
                    PublicationYear = 2022,
                    Description = "Những bí quyết thực tế để thành công trong kinh doanh",
                    CoverImageUrl = "/images/kinh-doanh-thanh-cong.jpg",
                    Quantity = 20,
                    AvailableQuantity = 18,
                    Price = 110000,
                    Language = "Tiếng Việt",
                    PageCount = 350,
                    AverageRating = 4.4f,
                    RatingCount = 22,
                    CreatedAt = DateTime.UtcNow
                },
                new Book
                {
                    Id = 6,
                    Title = "Lịch Sử Việt Nam",
                    Author = "Phạm Văn Đồng",
                    ISBN = "9786047770614",
                    Publisher = "Nhà Xuất Bản Chính Trị Quốc Gia",
                    PublicationYear = 2017,
                    Description = "Tổng quan về lịch sử Việt Nam từ cổ đại đến hiện đại",
                    CoverImageUrl = "/images/lich-su-viet-nam.jpg",
                    Quantity = 6,
                    AvailableQuantity = 5,
                    Price = 150000,
                    Language = "Tiếng Việt",
                    PageCount = 500,
                    AverageRating = 4.6f,
                    RatingCount = 28,
                    CreatedAt = DateTime.UtcNow
                },
                new Book
                {
                    Id = 7,
                    Title = "Công Thức Nấu Ăn Ngon",
                    Author = "Lê Thị C",
                    ISBN = "9786047770621",
                    Publisher = "Nhà Xuất Bản Phụ Nữ",
                    PublicationYear = 2023,
                    Description = "Bộ sưu tập công thức nấu ăn ngon và đơn giản",
                    CoverImageUrl = "/images/nau-an-ngon.jpg",
                    Quantity = 25,
                    AvailableQuantity = 22,
                    Price = 85000,
                    Language = "Tiếng Việt",
                    PageCount = 280,
                    AverageRating = 4.1f,
                    RatingCount = 20,
                    CreatedAt = DateTime.UtcNow
                },
                new Book
                {
                    Id = 8,
                    Title = "Truyện Cổ Tích Việt Nam",
                    Author = "Nhiều Tác Giả",
                    ISBN = "9786047770638",
                    Publisher = "Nhà Xuất Bản Kim Đồng",
                    PublicationYear = 2020,
                    Description = "Tuyển tập truyện cổ tích Việt Nam cho thiếu nhi",
                    CoverImageUrl = "/images/truyen-co-tich.jpg",
                    Quantity = 30,
                    AvailableQuantity = 28,
                    Price = 65000,
                    Language = "Tiếng Việt",
                    PageCount = 180,
                    AverageRating = 4.8f,
                    RatingCount = 35,
                    CreatedAt = DateTime.UtcNow
                }
            );

            // Seed BookCategories - Liên kết sách với danh mục
            modelBuilder.Entity<BookCategory>().HasData(
                new BookCategory { BookId = 1, CategoryId = 7 }, // Đắc Nhân Tâm -> Kinh Doanh
                new BookCategory { BookId = 2, CategoryId = 2 }, // Tôi Tài Giỏi -> Phi Tiểu Thuyết
                new BookCategory { BookId = 3, CategoryId = 8 }, // Tắt Đèn -> Văn Học Việt Nam
                new BookCategory { BookId = 4, CategoryId = 6 }, // Lập Trình C# -> Lập Trình
                new BookCategory { BookId = 5, CategoryId = 7 }, // Bí Quyết Kinh Doanh -> Kinh Doanh
                new BookCategory { BookId = 6, CategoryId = 4 }, // Lịch Sử Việt Nam -> Lịch Sử
                new BookCategory { BookId = 7, CategoryId = 10 }, // Công Thức Nấu Ăn -> Nấu Ăn
                new BookCategory { BookId = 8, CategoryId = 9 }  // Truyện Cổ Tích -> Thiếu Nhi
            );
        }
    }
}
