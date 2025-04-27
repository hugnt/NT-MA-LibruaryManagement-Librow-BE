using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Librow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSeedDataForUserAndDefaultCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Password",
                table: "User",
                newName: "PasswordHash");

            migrationBuilder.InsertData(
                table: "BookCategory",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new Guid("3f8b2a1e-5c4d-4e9f-a2b3-7c8d9e0f1a2b"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), "No Category", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000") });

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "Email", "Fullname", "PasswordHash", "Role", "UpdatedAt", "UpdatedBy", "Username" },
                values: new object[,]
                {
                    { new Guid("3f8b2a1e-5c4d-4e9f-a2b3-7c8d9e0f1a2b"), new DateTime(2025, 4, 25, 22, 33, 59, 894, DateTimeKind.Local).AddTicks(6329), new Guid("00000000-0000-0000-0000-000000000000"), "thanh.hung.st302@gmail.com", "Admin", "62D97E720D5574BBEB80B41144D1BC86648C78D747DDD4078C62E1E279B4D94D-F75BF08670B03F19CABE8AAD26B5763F", 0, new DateTime(2025, 4, 25, 22, 33, 59, 894, DateTimeKind.Local).AddTicks(6343), new Guid("00000000-0000-0000-0000-000000000000"), "admin" },
                    { new Guid("9a4c6e2d-8f3b-4d1a-b5c7-2e9f0a1b3c4d"), new DateTime(2025, 4, 25, 22, 33, 59, 894, DateTimeKind.Local).AddTicks(6349), new Guid("00000000-0000-0000-0000-000000000000"), "thanhhungst314@gmail.com", "User", "C40D0CF1F0815D27829F76BA3F7B0399A9FF5BD6C05252B7F500B6826419EE25-E41A6B82F54C202A240A483B224F15C3", 0, new DateTime(2025, 4, 25, 22, 33, 59, 894, DateTimeKind.Local).AddTicks(6350), new Guid("00000000-0000-0000-0000-000000000000"), "user001" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "BookCategory",
                keyColumn: "Id",
                keyValue: new Guid("3f8b2a1e-5c4d-4e9f-a2b3-7c8d9e0f1a2b"));

            migrationBuilder.DeleteData(
                table: "User",
                keyColumn: "Id",
                keyValue: new Guid("3f8b2a1e-5c4d-4e9f-a2b3-7c8d9e0f1a2b"));

            migrationBuilder.DeleteData(
                table: "User",
                keyColumn: "Id",
                keyValue: new Guid("9a4c6e2d-8f3b-4d1a-b5c7-2e9f0a1b3c4d"));

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "User",
                newName: "Password");
        }
    }
}
