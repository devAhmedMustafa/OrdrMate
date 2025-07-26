using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrdrMate.Migrations
{
    /// <inheritdoc />
    public partial class DeliverConfirmation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeliverRequest",
                columns: table => new
                {
                    OrderId = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliverRequest", x => x.OrderId);
                    table.ForeignKey(
                        name: "FK_DeliverRequest_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeliverRequest");
        }
    }
}
