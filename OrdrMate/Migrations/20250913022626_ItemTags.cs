using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrdrMate.Migrations
{
    /// <inheritdoc />
    public partial class ItemTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "Item",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "Item",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Item");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "Item");
        }
    }
}
