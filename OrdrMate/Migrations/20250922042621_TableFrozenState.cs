using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrdrMate.Migrations
{
    /// <inheritdoc />
    public partial class TableFrozenState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFrozen",
                table: "Table",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFrozen",
                table: "Table");
        }
    }
}
