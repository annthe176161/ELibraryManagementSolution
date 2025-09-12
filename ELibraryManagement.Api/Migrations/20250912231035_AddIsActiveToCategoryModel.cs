using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ELibraryManagement.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveToCategoryModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Categories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 23, 10, 35, 254, DateTimeKind.Utc).AddTicks(7310));

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 23, 10, 35, 254, DateTimeKind.Utc).AddTicks(7316));

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 23, 10, 35, 254, DateTimeKind.Utc).AddTicks(7321));

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 23, 10, 35, 254, DateTimeKind.Utc).AddTicks(7326));

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 23, 10, 35, 254, DateTimeKind.Utc).AddTicks(7330));

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 23, 10, 35, 254, DateTimeKind.Utc).AddTicks(7335));

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 23, 10, 35, 254, DateTimeKind.Utc).AddTicks(7339));

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 23, 10, 35, 254, DateTimeKind.Utc).AddTicks(7343));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "IsActive" },
                values: new object[] { new DateTime(2025, 9, 12, 23, 10, 35, 254, DateTimeKind.Utc).AddTicks(7232), true });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "IsActive" },
                values: new object[] { new DateTime(2025, 9, 12, 23, 10, 35, 254, DateTimeKind.Utc).AddTicks(7235), true });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "IsActive" },
                values: new object[] { new DateTime(2025, 9, 12, 23, 10, 35, 254, DateTimeKind.Utc).AddTicks(7238), true });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "IsActive" },
                values: new object[] { new DateTime(2025, 9, 12, 23, 10, 35, 254, DateTimeKind.Utc).AddTicks(7240), true });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "IsActive" },
                values: new object[] { new DateTime(2025, 9, 12, 23, 10, 35, 254, DateTimeKind.Utc).AddTicks(7242), true });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "IsActive" },
                values: new object[] { new DateTime(2025, 9, 12, 23, 10, 35, 254, DateTimeKind.Utc).AddTicks(7245), true });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "IsActive" },
                values: new object[] { new DateTime(2025, 9, 12, 23, 10, 35, 254, DateTimeKind.Utc).AddTicks(7247), true });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "IsActive" },
                values: new object[] { new DateTime(2025, 9, 12, 23, 10, 35, 254, DateTimeKind.Utc).AddTicks(7249), true });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CreatedAt", "IsActive" },
                values: new object[] { new DateTime(2025, 9, 12, 23, 10, 35, 254, DateTimeKind.Utc).AddTicks(7251), true });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CreatedAt", "IsActive" },
                values: new object[] { new DateTime(2025, 9, 12, 23, 10, 35, 254, DateTimeKind.Utc).AddTicks(7253), true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Categories");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 22, 33, 23, 942, DateTimeKind.Utc).AddTicks(7198));

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 22, 33, 23, 942, DateTimeKind.Utc).AddTicks(7204));

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 22, 33, 23, 942, DateTimeKind.Utc).AddTicks(7209));

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 22, 33, 23, 942, DateTimeKind.Utc).AddTicks(7213));

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 22, 33, 23, 942, DateTimeKind.Utc).AddTicks(7218));

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 22, 33, 23, 942, DateTimeKind.Utc).AddTicks(7222));

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 22, 33, 23, 942, DateTimeKind.Utc).AddTicks(7226));

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 22, 33, 23, 942, DateTimeKind.Utc).AddTicks(7230));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 22, 33, 23, 942, DateTimeKind.Utc).AddTicks(7070));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 22, 33, 23, 942, DateTimeKind.Utc).AddTicks(7073));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 22, 33, 23, 942, DateTimeKind.Utc).AddTicks(7076));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 22, 33, 23, 942, DateTimeKind.Utc).AddTicks(7079));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 22, 33, 23, 942, DateTimeKind.Utc).AddTicks(7081));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 22, 33, 23, 942, DateTimeKind.Utc).AddTicks(7083));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 22, 33, 23, 942, DateTimeKind.Utc).AddTicks(7085));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 22, 33, 23, 942, DateTimeKind.Utc).AddTicks(7087));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 22, 33, 23, 942, DateTimeKind.Utc).AddTicks(7089));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 22, 33, 23, 942, DateTimeKind.Utc).AddTicks(7091));
        }
    }
}
