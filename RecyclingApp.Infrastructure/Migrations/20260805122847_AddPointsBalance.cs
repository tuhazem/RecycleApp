using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecyclingApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPointsBalance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PointsBalance",
                table: "AspNetUsers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PointsBalance",
                table: "AspNetUsers");
        }
    }
}
