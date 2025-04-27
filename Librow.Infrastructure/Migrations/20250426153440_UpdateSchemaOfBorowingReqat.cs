using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Librow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSchemaOfBorowingReqat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BorrowCount",
                table: "BookBorrowingRequestDetails");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "User",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "BookRating",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "BookCategory",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "BookBorrowingRequest",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Book",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "BookCategory",
                keyColumn: "Id",
                keyValue: new Guid("3f8b2a1e-5c4d-4e9f-a2b3-7c8d9e0f1a2b"),
                columns: new[] { "CreatedAt", "IsDeleted", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: new Guid("3f8b2a1e-5c4d-4e9f-a2b3-7c8d9e0f1a2b"),
                columns: new[] { "CreatedAt", "IsDeleted", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: new Guid("9a4c6e2d-8f3b-4d1a-b5c7-2e9f0a1b3c4d"),
                columns: new[] { "CreatedAt", "IsDeleted", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "User");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "BookRating");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "BookCategory");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "BookBorrowingRequest");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Book");

            migrationBuilder.AddColumn<int>(
                name: "BorrowCount",
                table: "BookBorrowingRequestDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "BookCategory",
                keyColumn: "Id",
                keyValue: new Guid("3f8b2a1e-5c4d-4e9f-a2b3-7c8d9e0f1a2b"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: new Guid("3f8b2a1e-5c4d-4e9f-a2b3-7c8d9e0f1a2b"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 26, 2, 41, 4, 391, DateTimeKind.Local).AddTicks(9061), new DateTime(2025, 4, 26, 2, 41, 4, 391, DateTimeKind.Local).AddTicks(9077) });

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: new Guid("9a4c6e2d-8f3b-4d1a-b5c7-2e9f0a1b3c4d"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 26, 2, 41, 4, 391, DateTimeKind.Local).AddTicks(9086), new DateTime(2025, 4, 26, 2, 41, 4, 391, DateTimeKind.Local).AddTicks(9087) });
        }
    }
}
