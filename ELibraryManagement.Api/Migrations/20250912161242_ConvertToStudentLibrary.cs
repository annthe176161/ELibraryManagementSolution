using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ELibraryManagement.Api.Migrations
{
    /// <inheritdoc />
    public partial class ConvertToStudentLibrary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RentalPrice",
                table: "BorrowRecords");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Books");

            migrationBuilder.AddColumn<DateTime>(
                name: "ConfirmedDate",
                table: "BorrowRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 16, 12, 41, 170, DateTimeKind.Utc).AddTicks(6239));

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 16, 12, 41, 170, DateTimeKind.Utc).AddTicks(6245));

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 16, 12, 41, 170, DateTimeKind.Utc).AddTicks(6251));

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 16, 12, 41, 170, DateTimeKind.Utc).AddTicks(6256));

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 16, 12, 41, 170, DateTimeKind.Utc).AddTicks(6260));

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 16, 12, 41, 170, DateTimeKind.Utc).AddTicks(6265));

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 16, 12, 41, 170, DateTimeKind.Utc).AddTicks(6270));

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 16, 12, 41, 170, DateTimeKind.Utc).AddTicks(6274));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 16, 12, 41, 170, DateTimeKind.Utc).AddTicks(6087));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 16, 12, 41, 170, DateTimeKind.Utc).AddTicks(6090));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 16, 12, 41, 170, DateTimeKind.Utc).AddTicks(6093));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 16, 12, 41, 170, DateTimeKind.Utc).AddTicks(6096));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 16, 12, 41, 170, DateTimeKind.Utc).AddTicks(6099));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 16, 12, 41, 170, DateTimeKind.Utc).AddTicks(6101));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 16, 12, 41, 170, DateTimeKind.Utc).AddTicks(6104));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 16, 12, 41, 170, DateTimeKind.Utc).AddTicks(6106));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 16, 12, 41, 170, DateTimeKind.Utc).AddTicks(6109));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 16, 12, 41, 170, DateTimeKind.Utc).AddTicks(6111));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfirmedDate",
                table: "BorrowRecords");

            migrationBuilder.AddColumn<decimal>(
                name: "RentalPrice",
                table: "BorrowRecords",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Books",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Price" },
                values: new object[] { new DateTime(2025, 9, 11, 10, 46, 50, 719, DateTimeKind.Utc).AddTicks(8728), 89000m });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "Price" },
                values: new object[] { new DateTime(2025, 9, 11, 10, 46, 50, 719, DateTimeKind.Utc).AddTicks(8732), 95000m });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "Price" },
                values: new object[] { new DateTime(2025, 9, 11, 10, 46, 50, 719, DateTimeKind.Utc).AddTicks(8735), 45000m });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "Price" },
                values: new object[] { new DateTime(2025, 9, 11, 10, 46, 50, 719, DateTimeKind.Utc).AddTicks(8738), 120000m });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "Price" },
                values: new object[] { new DateTime(2025, 9, 11, 10, 46, 50, 719, DateTimeKind.Utc).AddTicks(8740), 110000m });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "Price" },
                values: new object[] { new DateTime(2025, 9, 11, 10, 46, 50, 719, DateTimeKind.Utc).AddTicks(8743), 150000m });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "Price" },
                values: new object[] { new DateTime(2025, 9, 11, 10, 46, 50, 719, DateTimeKind.Utc).AddTicks(8746), 85000m });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "Price" },
                values: new object[] { new DateTime(2025, 9, 11, 10, 46, 50, 719, DateTimeKind.Utc).AddTicks(8748), 65000m });

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
    }
}
