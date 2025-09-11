using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ELibraryManagement.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBookCoverUrls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Roles already seeded in original seed - skip inserting to avoid duplicates

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CoverImageUrl", "CreatedAt" },
                values: new object[] { "https://cdn1.fahasa.com/media/catalog/product/d/n/dntttttuntitled.jpg", new DateTime(2025, 9, 11, 10, 46, 50, 719, DateTimeKind.Utc).AddTicks(8728) });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CoverImageUrl", "CreatedAt" },
                values: new object[] { "https://lh4.googleusercontent.com/proxy/92QuMwJnEjAGTfNMAB9joNXoouO9NuduIgBPaKtL0h0UPvaeTFj3Xef967P3mbrE7F1J5cfnvn2PKw8cwIINHMhxf9L2C3bPRQ2Ef14EVeZAIb_rdt3WzLOb98FXMVhAs2lNuT9ABlcODTeUqt5z27FQ8fQE4ZtQEw", new DateTime(2025, 9, 11, 10, 46, 50, 719, DateTimeKind.Utc).AddTicks(8732) });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CoverImageUrl", "CreatedAt" },
                values: new object[] { "https://upload.wikimedia.org/wikipedia/vi/b/b1/T%E1%BA%AFt_%C4%91%C3%A8n-Nh%C3%A3_Nam.jpeg", new DateTime(2025, 9, 11, 10, 46, 50, 719, DateTimeKind.Utc).AddTicks(8735) });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CoverImageUrl", "CreatedAt" },
                values: new object[] { "https://images.nxbbachkhoa.vn/Picture/2024/5/8/image-20240508180323597.jpg", new DateTime(2025, 9, 11, 10, 46, 50, 719, DateTimeKind.Utc).AddTicks(8738) });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CoverImageUrl", "CreatedAt" },
                values: new object[] { "https://cdn1.fahasa.com/media/catalog/product/8/9/8935236401296.jpg", new DateTime(2025, 9, 11, 10, 46, 50, 719, DateTimeKind.Utc).AddTicks(8740) });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CoverImageUrl", "CreatedAt" },
                values: new object[] { "https://sachvuii.com/wp-content/uploads/2024/06/Ebook-Lich-su-Viet-Nam.jpg", new DateTime(2025, 9, 11, 10, 46, 50, 719, DateTimeKind.Utc).AddTicks(8743) });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CoverImageUrl", "CreatedAt" },
                values: new object[] { "https://salt.tikicdn.com/cache/w1200/ts/product/6d/30/f5/88c01835d4b7107e03373bcc346c028f.jpg", new DateTime(2025, 9, 11, 10, 46, 50, 719, DateTimeKind.Utc).AddTicks(8746) });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CoverImageUrl", "CreatedAt" },
                values: new object[] { "https://product.hstatic.net/1000237375/product/100-truyen-co-tich-viet-nam-440.jpg", new DateTime(2025, 9, 11, 10, 46, 50, 719, DateTimeKind.Utc).AddTicks(8748) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 11, 10, 46, 50, 719, DateTimeKind.Utc).AddTicks(8623));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 11, 10, 46, 50, 719, DateTimeKind.Utc).AddTicks(8625));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 11, 10, 46, 50, 719, DateTimeKind.Utc).AddTicks(8626));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 11, 10, 46, 50, 719, DateTimeKind.Utc).AddTicks(8628));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 11, 10, 46, 50, 719, DateTimeKind.Utc).AddTicks(8629));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 11, 10, 46, 50, 719, DateTimeKind.Utc).AddTicks(8630));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 11, 10, 46, 50, 719, DateTimeKind.Utc).AddTicks(8631));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 11, 10, 46, 50, 719, DateTimeKind.Utc).AddTicks(8685));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 11, 10, 46, 50, 719, DateTimeKind.Utc).AddTicks(8686));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 11, 10, 46, 50, 719, DateTimeKind.Utc).AddTicks(8687));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No role deletions - roles are managed by initial seeder

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CoverImageUrl", "CreatedAt" },
                values: new object[] { "/images/dac-nhan-tam.jpg", new DateTime(2025, 9, 10, 14, 43, 9, 244, DateTimeKind.Utc).AddTicks(4623) });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CoverImageUrl", "CreatedAt" },
                values: new object[] { "/images/toi-tai-gioi.jpg", new DateTime(2025, 9, 10, 14, 43, 9, 244, DateTimeKind.Utc).AddTicks(4627) });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CoverImageUrl", "CreatedAt" },
                values: new object[] { "/images/tat-den.jpg", new DateTime(2025, 9, 10, 14, 43, 9, 244, DateTimeKind.Utc).AddTicks(4630) });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CoverImageUrl", "CreatedAt" },
                values: new object[] { "/images/csharp-basic.jpg", new DateTime(2025, 9, 10, 14, 43, 9, 244, DateTimeKind.Utc).AddTicks(4634) });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CoverImageUrl", "CreatedAt" },
                values: new object[] { "/images/kinh-doanh-thanh-cong.jpg", new DateTime(2025, 9, 10, 14, 43, 9, 244, DateTimeKind.Utc).AddTicks(4637) });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CoverImageUrl", "CreatedAt" },
                values: new object[] { "/images/lich-su-viet-nam.jpg", new DateTime(2025, 9, 10, 14, 43, 9, 244, DateTimeKind.Utc).AddTicks(4639) });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CoverImageUrl", "CreatedAt" },
                values: new object[] { "/images/nau-an-ngon.jpg", new DateTime(2025, 9, 10, 14, 43, 9, 244, DateTimeKind.Utc).AddTicks(4642) });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CoverImageUrl", "CreatedAt" },
                values: new object[] { "/images/truyen-co-tich.jpg", new DateTime(2025, 9, 10, 14, 43, 9, 244, DateTimeKind.Utc).AddTicks(4644) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 10, 14, 43, 9, 244, DateTimeKind.Utc).AddTicks(4423));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 10, 14, 43, 9, 244, DateTimeKind.Utc).AddTicks(4425));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 10, 14, 43, 9, 244, DateTimeKind.Utc).AddTicks(4427));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 10, 14, 43, 9, 244, DateTimeKind.Utc).AddTicks(4428));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 10, 14, 43, 9, 244, DateTimeKind.Utc).AddTicks(4456));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 10, 14, 43, 9, 244, DateTimeKind.Utc).AddTicks(4457));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 10, 14, 43, 9, 244, DateTimeKind.Utc).AddTicks(4459));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 10, 14, 43, 9, 244, DateTimeKind.Utc).AddTicks(4460));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 10, 14, 43, 9, 244, DateTimeKind.Utc).AddTicks(4461));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 10, 14, 43, 9, 244, DateTimeKind.Utc).AddTicks(4462));
        }
    }
}
