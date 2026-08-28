using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecyclingApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "RecyclingTransactions",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Completed");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "RecyclingTransactions");
        }
    }
}
