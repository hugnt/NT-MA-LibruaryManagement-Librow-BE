using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Librow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixSeedDataForUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                columns: new[] { "CreatedAt", "Fullname", "Role", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 25, 22, 53, 58, 582, DateTimeKind.Local).AddTicks(1602), "Nguyen Thanh User", 1, new DateTime(2025, 4, 25, 22, 53, 58, 582, DateTimeKind.Local).AddTicks(1603) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: new Guid("3f8b2a1e-5c4d-4e9f-a2b3-7c8d9e0f1a2b"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 25, 22, 33, 59, 894, DateTimeKind.Local).AddTicks(6329), new DateTime(2025, 4, 25, 22, 33, 59, 894, DateTimeKind.Local).AddTicks(6343) });

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: new Guid("9a4c6e2d-8f3b-4d1a-b5c7-2e9f0a1b3c4d"),
                columns: new[] { "CreatedAt", "Fullname", "Role", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 25, 22, 33, 59, 894, DateTimeKind.Local).AddTicks(6349), "User", 0, new DateTime(2025, 4, 25, 22, 33, 59, 894, DateTimeKind.Local).AddTicks(6350) });
        }
    }
}
