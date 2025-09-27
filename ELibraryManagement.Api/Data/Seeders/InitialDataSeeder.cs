using ELibraryManagement.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ELibraryManagement.Api.Data.Seeders
{
    public static class InitialDataSeeder
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
            var seedDate = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Tiểu Thuyết", Description = "Các tác phẩm tiểu thuyết, truyện dài", Color = "#FF6B6B", CreatedAt = seedDate },
                new Category { Id = 2, Name = "Phi Tiểu Thuyết", Description = "Sách phi hư cấu, sách thực tế", Color = "#4ECDC4", CreatedAt = seedDate },
                new Category { Id = 3, Name = "Khoa Học", Description = "Sách khoa học và công nghệ", Color = "#45B7D1", CreatedAt = seedDate },
                new Category { Id = 4, Name = "Lịch Sử", Description = "Sách lịch sử và văn hóa", Color = "#96CEB4", CreatedAt = seedDate },
                new Category { Id = 5, Name = "Tiểu Sử", Description = "Tiểu sử và hồi ký", Color = "#FECA57", CreatedAt = seedDate },
                new Category { Id = 6, Name = "Lập Trình", Description = "Sách lập trình và phát triển phần mềm", Color = "#FF9FF3", CreatedAt = seedDate },
                new Category { Id = 7, Name = "Kinh Doanh", Description = "Sách kinh doanh và kinh tế", Color = "#54A0FF", CreatedAt = seedDate },
                new Category { Id = 8, Name = "Văn Học Việt Nam", Description = "Tác phẩm văn học Việt Nam", Color = "#FF8A65", CreatedAt = seedDate },
                new Category { Id = 9, Name = "Thiếu Nhi", Description = "Sách dành cho thiếu nhi", Color = "#81C784", CreatedAt = seedDate },
                new Category { Id = 10, Name = "Nấu Ăn", Description = "Sách công thức nấu ăn", Color = "#FFB74D", CreatedAt = seedDate }
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
                    CoverImageUrl = "https://cdn1.fahasa.com/media/catalog/product/d/n/dntttttuntitled.jpg",
                    Quantity = 10,
                    AvailableQuantity = 10,
                    Language = "Tiếng Việt",
                    PageCount = 320,
                    AverageRating = 0f,
                    RatingCount = 0,
                    CreatedAt = seedDate
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
                    CoverImageUrl = "https://lh4.googleusercontent.com/proxy/92QuMwJnEjAGTfNMAB9joNXoouO9NuduIgBPaKtL0h0UPvaeTFj3Xef967P3mbrE7F1J5cfnvn2PKw8cwIINHMhxf9L2C3bPRQ2Ef14EVeZAIb_rdt3WzLOb98FXMVhAs2lNuT9ABlcODTeUqt5z27FQ8fQE4ZtQEw",
                    Quantity = 15,
                    AvailableQuantity = 15,
                    Language = "Tiếng Việt",
                    PageCount = 280,
                    AverageRating = 0f,
                    RatingCount = 0,
                    CreatedAt = seedDate
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
                    CoverImageUrl = "https://upload.wikimedia.org/wikipedia/vi/b/b1/T%E1%BA%AFt_%C4%91%C3%A8n-Nh%C3%A3_Nam.jpeg",
                    Quantity = 8,
                    AvailableQuantity = 8,
                    Language = "Tiếng Việt",
                    PageCount = 200,
                    AverageRating = 0f,
                    RatingCount = 0,
                    CreatedAt = seedDate
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
                    CoverImageUrl = "https://images.nxbbachkhoa.vn/Picture/2024/5/8/image-20240508180323597.jpg",
                    Quantity = 12,
                    AvailableQuantity = 12,
                    Language = "Tiếng Việt",
                    PageCount = 400,
                    AverageRating = 0f,
                    RatingCount = 0,
                    CreatedAt = seedDate
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
                    CoverImageUrl = "https://cdn1.fahasa.com/media/catalog/product/8/9/8935236401296.jpg",
                    Quantity = 20,
                    AvailableQuantity = 20,
                    Language = "Tiếng Việt",
                    PageCount = 350,
                    AverageRating = 0f,
                    RatingCount = 0,
                    CreatedAt = seedDate
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
                    CoverImageUrl = "https://sachvuii.com/wp-content/uploads/2024/06/Ebook-Lich-su-Viet-Nam.jpg",
                    Quantity = 6,
                    AvailableQuantity = 6,
                    Language = "Tiếng Việt",
                    PageCount = 500,
                    AverageRating = 0f,
                    RatingCount = 0,
                    CreatedAt = seedDate
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
                    CoverImageUrl = "https://salt.tikicdn.com/cache/w1200/ts/product/6d/30/f5/88c01835d4b7107e03373bcc346c028f.jpg",
                    Quantity = 25,
                    AvailableQuantity = 25,
                    Language = "Tiếng Việt",
                    PageCount = 280,
                    AverageRating = 0f,
                    RatingCount = 0,
                    CreatedAt = seedDate
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
                    CoverImageUrl = "https://product.hstatic.net/1000237375/product/100-truyen-co-tich-viet-nam-440.jpg",
                    Quantity = 30,
                    AvailableQuantity = 30,
                    Language = "Tiếng Việt",
                    PageCount = 180,
                    AverageRating = 0f,
                    RatingCount = 0,
                    CreatedAt = seedDate
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
