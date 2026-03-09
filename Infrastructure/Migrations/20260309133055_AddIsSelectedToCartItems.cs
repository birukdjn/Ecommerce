using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsSelectedToCartItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSelected",
                table: "OrderItems");

            migrationBuilder.AddColumn<bool>(
                name: "IsSelected",
                table: "CartItems",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSelected",
                table: "CartItems");

            migrationBuilder.AddColumn<bool>(
                name: "IsSelected",
                table: "OrderItems",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
