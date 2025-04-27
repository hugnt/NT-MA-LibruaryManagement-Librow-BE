using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Librow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTableRefreshToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RefreshToken",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JwtId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpireAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshToken", x => x.Id);
                });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefreshToken");

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: new Guid("3f8b2a1e-5c4d-4e9f-a2b3-7c8d9e0f1a2b"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 25, 22, 53, 58, 582, DateTimeKind.Local).AddTicks(1553), new DateTime(2025, 4, 25, 22, 53, 58, 582, DateTimeKind.Local).AddTicks(1597) });

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: new Guid("9a4c6e2d-8f3b-4d1a-b5c7-2e9f0a1b3c4d"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 25, 22, 53, 58, 582, DateTimeKind.Local).AddTicks(1602), new DateTime(2025, 4, 25, 22, 53, 58, 582, DateTimeKind.Local).AddTicks(1603) });
        }
    }
}
