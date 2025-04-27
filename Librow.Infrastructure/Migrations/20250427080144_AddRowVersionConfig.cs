using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Librow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRowVersionConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "BookBorrowingRequestDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Book",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "BookBorrowingRequestDetails");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Book");
        }
    }
}
