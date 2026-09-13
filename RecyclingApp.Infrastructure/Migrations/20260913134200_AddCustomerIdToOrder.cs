using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecyclingApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerIdToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomerId",
                table: "Orders",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            // Ensure any existing dummy/test orders have a valid CustomerId from AspNetUsers before applying FK constraint
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM [AspNetUsers])
                BEGIN
                    UPDATE [Orders]
                    SET [CustomerId] = (SELECT TOP 1 [Id] FROM [AspNetUsers])
                    WHERE [CustomerId] = '' OR [CustomerId] NOT IN (SELECT [Id] FROM [AspNetUsers]);
                END
                ELSE
                BEGIN
                    DELETE FROM [OrderItems];
                    DELETE FROM [Orders];
                END
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerId",
                table: "Orders",
                column: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_AspNetUsers_CustomerId",
                table: "Orders",
                column: "CustomerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_AspNetUsers_CustomerId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_CustomerId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "Orders");
        }
    }
}
