using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrdrMate.Migrations
{
    /// <inheritdoc />
    public partial class Delivery_Takeaway_Support : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DeliveryEnabled",
                table: "Branch",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TakeAwayEnabled",
                table: "Branch",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryEnabled",
                table: "Branch");

            migrationBuilder.DropColumn(
                name: "TakeAwayEnabled",
                table: "Branch");
        }
    }
}
