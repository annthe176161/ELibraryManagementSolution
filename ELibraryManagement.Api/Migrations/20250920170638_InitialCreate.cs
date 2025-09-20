using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ELibraryManagement.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StudentId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AvatarUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastLoginDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Author = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ISBN = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: true),
                    Publisher = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PublicationYear = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CoverImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    AvailableQuantity = table.Column<int>(type: "int", nullable: false),
                    Language = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PageCount = table.Column<int>(type: "int", nullable: false),
                    AverageRating = table.Column<double>(type: "float", nullable: false),
                    RatingCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserStatuses",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AccountStatus = table.Column<int>(type: "int", nullable: false),
                    TotalOutstandingFines = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OverdueFinesCount = table.Column<int>(type: "int", nullable: false),
                    MaxBorrowLimit = table.Column<int>(type: "int", nullable: false),
                    CurrentBorrowCount = table.Column<int>(type: "int", nullable: false),
                    BlockReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BlockedUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserStatuses", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserStatuses_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BorrowRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BookId = table.Column<int>(type: "int", nullable: false),
                    BorrowDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReturnDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExtensionCount = table.Column<int>(type: "int", nullable: false),
                    LastExtensionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConfirmedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BorrowRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BorrowRecords_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BorrowRecords_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BookId = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ReviewDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reviews_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reviews_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookCategories",
                columns: table => new
                {
                    BookId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookCategories", x => new { x.BookId, x.CategoryId });
                    table.ForeignKey(
                        name: "FK_BookCategories_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Fines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BorrowRecordId = table.Column<int>(type: "int", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    FineDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaidDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReminderCount = table.Column<int>(type: "int", nullable: false),
                    LastReminderDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EscalationReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EscalationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fines_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Fines_BorrowRecords_BorrowRecordId",
                        column: x => x.BorrowRecordId,
                        principalTable: "BorrowRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "FineActionHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FineId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ActionType = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FineActionHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FineActionHistories_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FineActionHistories_Fines_FineId",
                        column: x => x.FineId,
                        principalTable: "Fines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "1", null, "Admin", "ADMIN" },
                    { "2", null, "Librarian", "LIBRARIAN" },
                    { "3", null, "User", "USER" }
                });

            migrationBuilder.InsertData(
                table: "Books",
                columns: new[] { "Id", "Author", "AvailableQuantity", "AverageRating", "CoverImageUrl", "CreatedAt", "Description", "ISBN", "IsDeleted", "Language", "PageCount", "PublicationYear", "Publisher", "Quantity", "RatingCount", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "Dale Carnegie", 10, 0.0, "https://cdn1.fahasa.com/media/catalog/product/d/n/dntttttuntitled.jpg", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Cuốn sách về nghệ thuật giao tiếp và thu phục lòng người", "9786047770560", false, "Tiếng Việt", 320, 2020, "Nhà Xuất Bản Tổng Hợp TP.HCM", 10, 0, "Đắc Nhân Tâm", null },
                    { 2, "Adam Khoo", 15, 0.0, "https://lh4.googleusercontent.com/proxy/92QuMwJnEjAGTfNMAB9joNXoouO9NuduIgBPaKtL0h0UPvaeTFj3Xef967P3mbrE7F1J5cfnvn2PKw8cwIINHMhxf9L2C3bPRQ2Ef14EVeZAIb_rdt3WzLOb98FXMVhAs2lNuT9ABlcODTeUqt5z27FQ8fQE4ZtQEw", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Sách về phát triển bản thân và thành công", "9786047770577", false, "Tiếng Việt", 280, 2019, "Nhà Xuất Bản Trẻ", 15, 0, "Tôi Tài Giỏi, Bạn Cũng Thế", null },
                    { 3, "Ngô Tất Tố", 8, 0.0, "https://upload.wikimedia.org/wikipedia/vi/b/b1/T%E1%BA%AFt_%C4%91%C3%A8n-Nh%C3%A3_Nam.jpeg", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tác phẩm kinh điển của văn học Việt Nam", "9786047770584", false, "Tiếng Việt", 200, 2018, "Nhà Xuất Bản Giáo Dục", 8, 0, "Tắt Đèn", null },
                    { 4, "Nguyễn Văn A", 12, 0.0, "https://images.nxbbachkhoa.vn/Picture/2024/5/8/image-20240508180323597.jpg", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Sách hướng dẫn lập trình C# từ cơ bản đến nâng cao", "9786047770591", false, "Tiếng Việt", 400, 2021, "Nhà Xuất Bản Bách Khoa", 12, 0, "Lập Trình C# Cơ Bản", null },
                    { 5, "Trần Thị B", 20, 0.0, "https://cdn1.fahasa.com/media/catalog/product/8/9/8935236401296.jpg", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Những bí quyết thực tế để thành công trong kinh doanh", "9786047770607", false, "Tiếng Việt", 350, 2022, "Nhà Xuất Bản Lao Động", 20, 0, "Bí Quyết Thành Công Trong Kinh Doanh", null },
                    { 6, "Phạm Văn Đồng", 6, 0.0, "https://sachvuii.com/wp-content/uploads/2024/06/Ebook-Lich-su-Viet-Nam.jpg", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tổng quan về lịch sử Việt Nam từ cổ đại đến hiện đại", "9786047770614", false, "Tiếng Việt", 500, 2017, "Nhà Xuất Bản Chính Trị Quốc Gia", 6, 0, "Lịch Sử Việt Nam", null },
                    { 7, "Lê Thị C", 25, 0.0, "https://salt.tikicdn.com/cache/w1200/ts/product/6d/30/f5/88c01835d4b7107e03373bcc346c028f.jpg", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bộ sưu tập công thức nấu ăn ngon và đơn giản", "9786047770621", false, "Tiếng Việt", 280, 2023, "Nhà Xuất Bản Phụ Nữ", 25, 0, "Công Thức Nấu Ăn Ngon", null },
                    { 8, "Nhiều Tác Giả", 30, 0.0, "https://product.hstatic.net/1000237375/product/100-truyen-co-tich-viet-nam-440.jpg", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tuyển tập truyện cổ tích Việt Nam cho thiếu nhi", "9786047770638", false, "Tiếng Việt", 180, 2020, "Nhà Xuất Bản Kim Đồng", 30, 0, "Truyện Cổ Tích Việt Nam", null }
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Color", "CreatedAt", "Description", "IsActive", "IsDeleted", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "#FF6B6B", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Các tác phẩm tiểu thuyết, truyện dài", true, false, "Tiểu Thuyết", null },
                    { 2, "#4ECDC4", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Sách phi hư cấu, sách thực tế", true, false, "Phi Tiểu Thuyết", null },
                    { 3, "#45B7D1", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Sách khoa học và công nghệ", true, false, "Khoa Học", null },
                    { 4, "#96CEB4", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Sách lịch sử và văn hóa", true, false, "Lịch Sử", null },
                    { 5, "#FECA57", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tiểu sử và hồi ký", true, false, "Tiểu Sử", null },
                    { 6, "#FF9FF3", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Sách lập trình và phát triển phần mềm", true, false, "Lập Trình", null },
                    { 7, "#54A0FF", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Sách kinh doanh và kinh tế", true, false, "Kinh Doanh", null },
                    { 8, "#FF8A65", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tác phẩm văn học Việt Nam", true, false, "Văn Học Việt Nam", null },
                    { 9, "#81C784", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Sách dành cho thiếu nhi", true, false, "Thiếu Nhi", null },
                    { 10, "#FFB74D", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Sách công thức nấu ăn", true, false, "Nấu Ăn", null }
                });

            migrationBuilder.InsertData(
                table: "BookCategories",
                columns: new[] { "BookId", "CategoryId" },
                values: new object[,]
                {
                    { 1, 7 },
                    { 2, 2 },
                    { 3, 8 },
                    { 4, 6 },
                    { 5, 7 },
                    { 6, 4 },
                    { 7, 10 },
                    { 8, 9 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BookCategories_CategoryId",
                table: "BookCategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Books_ISBN",
                table: "Books",
                column: "ISBN",
                unique: true,
                filter: "[ISBN] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Books_Title_Author",
                table: "Books",
                columns: new[] { "Title", "Author" });

            migrationBuilder.CreateIndex(
                name: "IX_BorrowRecords_BookId",
                table: "BorrowRecords",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_BorrowRecords_UserId_BookId_BorrowDate",
                table: "BorrowRecords",
                columns: new[] { "UserId", "BookId", "BorrowDate" });

            migrationBuilder.CreateIndex(
                name: "IX_FineActionHistories_FineId",
                table: "FineActionHistories",
                column: "FineId");

            migrationBuilder.CreateIndex(
                name: "IX_FineActionHistories_UserId",
                table: "FineActionHistories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Fines_BorrowRecordId",
                table: "Fines",
                column: "BorrowRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_Fines_UserId",
                table: "Fines",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_BookId_UserId",
                table: "Reviews",
                columns: new[] { "BookId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_UserId",
                table: "Reviews",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "BookCategories");

            migrationBuilder.DropTable(
                name: "FineActionHistories");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "UserStatuses");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Fines");

            migrationBuilder.DropTable(
                name: "BorrowRecords");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Books");
        }
    }
}
