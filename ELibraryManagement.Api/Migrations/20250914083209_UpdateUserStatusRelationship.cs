using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ELibraryManagement.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserStatusRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserStatuses_AspNetUsers_UserId1",
                table: "UserStatuses");

            migrationBuilder.DropIndex(
                name: "IX_UserStatuses_UserId1",
                table: "UserStatuses");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "UserStatuses");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 14, 8, 32, 8, 958, DateTimeKind.Utc).AddTicks(49));

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 14, 8, 32, 8, 958, DateTimeKind.Utc).AddTicks(53));

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 14, 8, 32, 8, 958, DateTimeKind.Utc).AddTicks(56));

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 14, 8, 32, 8, 958, DateTimeKind.Utc).AddTicks(59));

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 14, 8, 32, 8, 958, DateTimeKind.Utc).AddTicks(61));

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 14, 8, 32, 8, 958, DateTimeKind.Utc).AddTicks(66));

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 14, 8, 32, 8, 958, DateTimeKind.Utc).AddTicks(71));

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 14, 8, 32, 8, 958, DateTimeKind.Utc).AddTicks(75));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 14, 8, 32, 8, 957, DateTimeKind.Utc).AddTicks(9973));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 14, 8, 32, 8, 957, DateTimeKind.Utc).AddTicks(9975));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 14, 8, 32, 8, 957, DateTimeKind.Utc).AddTicks(9977));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 14, 8, 32, 8, 957, DateTimeKind.Utc).AddTicks(9978));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 14, 8, 32, 8, 957, DateTimeKind.Utc).AddTicks(9980));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 14, 8, 32, 8, 957, DateTimeKind.Utc).AddTicks(9981));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 14, 8, 32, 8, 957, DateTimeKind.Utc).AddTicks(9982));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 14, 8, 32, 8, 957, DateTimeKind.Utc).AddTicks(9984));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 14, 8, 32, 8, 957, DateTimeKind.Utc).AddTicks(9985));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 14, 8, 32, 8, 957, DateTimeKind.Utc).AddTicks(9987));

            migrationBuilder.AddForeignKey(
                name: "FK_UserStatuses_AspNetUsers_UserId",
                table: "UserStatuses",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserStatuses_AspNetUsers_UserId",
                table: "UserStatuses");

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "UserStatuses",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 14, 8, 31, 28, 677, DateTimeKind.Utc).AddTicks(5181));

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 14, 8, 31, 28, 677, DateTimeKind.Utc).AddTicks(5186));

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 14, 8, 31, 28, 677, DateTimeKind.Utc).AddTicks(5262));

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 14, 8, 31, 28, 677, DateTimeKind.Utc).AddTicks(5266));

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 14, 8, 31, 28, 677, DateTimeKind.Utc).AddTicks(5270));

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 14, 8, 31, 28, 677, DateTimeKind.Utc).AddTicks(5273));

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 14, 8, 31, 28, 677, DateTimeKind.Utc).AddTicks(5277));

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 14, 8, 31, 28, 677, DateTimeKind.Utc).AddTicks(5280));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 14, 8, 31, 28, 677, DateTimeKind.Utc).AddTicks(5119));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 14, 8, 31, 28, 677, DateTimeKind.Utc).AddTicks(5122));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 14, 8, 31, 28, 677, DateTimeKind.Utc).AddTicks(5124));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 14, 8, 31, 28, 677, DateTimeKind.Utc).AddTicks(5126));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 14, 8, 31, 28, 677, DateTimeKind.Utc).AddTicks(5127));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 14, 8, 31, 28, 677, DateTimeKind.Utc).AddTicks(5129));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 14, 8, 31, 28, 677, DateTimeKind.Utc).AddTicks(5131));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 14, 8, 31, 28, 677, DateTimeKind.Utc).AddTicks(5132));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 14, 8, 31, 28, 677, DateTimeKind.Utc).AddTicks(5134));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 14, 8, 31, 28, 677, DateTimeKind.Utc).AddTicks(5135));

            migrationBuilder.CreateIndex(
                name: "IX_UserStatuses_UserId1",
                table: "UserStatuses",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_UserStatuses_AspNetUsers_UserId1",
                table: "UserStatuses",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
