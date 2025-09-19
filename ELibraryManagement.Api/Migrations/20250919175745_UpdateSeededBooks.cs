using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ELibraryManagement.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSeededBooks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AvailableQuantity", "AverageRating", "RatingCount" },
                values: new object[] { 10, 0.0, 0 });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "AvailableQuantity", "AverageRating", "RatingCount" },
                values: new object[] { 15, 0.0, 0 });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "AvailableQuantity", "AverageRating", "RatingCount" },
                values: new object[] { 8, 0.0, 0 });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "AvailableQuantity", "AverageRating", "RatingCount" },
                values: new object[] { 12, 0.0, 0 });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "AvailableQuantity", "AverageRating", "RatingCount" },
                values: new object[] { 20, 0.0, 0 });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "AvailableQuantity", "AverageRating", "RatingCount" },
                values: new object[] { 6, 0.0, 0 });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "AvailableQuantity", "AverageRating", "RatingCount" },
                values: new object[] { 25, 0.0, 0 });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "AvailableQuantity", "AverageRating", "RatingCount" },
                values: new object[] { 30, 0.0, 0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AvailableQuantity", "AverageRating", "RatingCount" },
                values: new object[] { 8, 4.5, 25 });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "AvailableQuantity", "AverageRating", "RatingCount" },
                values: new object[] { 12, 4.3000001907348633, 18 });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "AvailableQuantity", "AverageRating", "RatingCount" },
                values: new object[] { 6, 4.6999998092651367, 32 });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "AvailableQuantity", "AverageRating", "RatingCount" },
                values: new object[] { 10, 4.1999998092651367, 15 });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "AvailableQuantity", "AverageRating", "RatingCount" },
                values: new object[] { 18, 4.4000000953674316, 22 });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "AvailableQuantity", "AverageRating", "RatingCount" },
                values: new object[] { 5, 4.5999999046325684, 28 });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "AvailableQuantity", "AverageRating", "RatingCount" },
                values: new object[] { 22, 4.0999999046325684, 20 });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "AvailableQuantity", "AverageRating", "RatingCount" },
                values: new object[] { 28, 4.8000001907348633, 35 });
        }
    }
}
