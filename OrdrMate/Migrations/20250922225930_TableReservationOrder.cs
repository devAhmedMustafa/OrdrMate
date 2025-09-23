using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrdrMate.Migrations
{
    /// <inheritdoc />
    public partial class TableReservationOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TableReservation_Order_OrderId",
                table: "TableReservation");

            migrationBuilder.DropTable(
                name: "Indoor");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Takeaway",
                table: "Takeaway");

            migrationBuilder.DropIndex(
                name: "IX_TableReservation_OrderId",
                table: "TableReservation");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "TableReservation");

            migrationBuilder.AddColumn<string>(
                name: "Id",
                table: "Takeaway",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TableReservationId",
                table: "Order",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TakeawayId",
                table: "Order",
                type: "text",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Takeaway",
                table: "Takeaway",
                columns: new[] { "Id", "OrderId" });

            migrationBuilder.CreateIndex(
                name: "IX_Takeaway_OrderId",
                table: "Takeaway",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Order_TableReservationId",
                table: "Order",
                column: "TableReservationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Order_TableReservation_TableReservationId",
                table: "Order",
                column: "TableReservationId",
                principalTable: "TableReservation",
                principalColumn: "ReservationId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Order_TableReservation_TableReservationId",
                table: "Order");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Takeaway",
                table: "Takeaway");

            migrationBuilder.DropIndex(
                name: "IX_Takeaway_OrderId",
                table: "Takeaway");

            migrationBuilder.DropIndex(
                name: "IX_Order_TableReservationId",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Takeaway");

            migrationBuilder.DropColumn(
                name: "IsFrozen",
                table: "Table");

            migrationBuilder.DropColumn(
                name: "OrderTax",
                table: "Restaurant");

            migrationBuilder.DropColumn(
                name: "TableReservationId",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "TakeawayId",
                table: "Order");

            migrationBuilder.AddColumn<string>(
                name: "OrderId",
                table: "TableReservation",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Takeaway",
                table: "Takeaway",
                columns: new[] { "OrderId", "OrderNumber" });

            migrationBuilder.CreateTable(
                name: "Indoor",
                columns: table => new
                {
                    TableNumber = table.Column<int>(type: "integer", nullable: false),
                    BranchId = table.Column<string>(type: "text", nullable: false),
                    OrderId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Indoor", x => new { x.TableNumber, x.BranchId, x.OrderId });
                    table.ForeignKey(
                        name: "FK_Indoor_Branch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Indoor_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TableReservation_OrderId",
                table: "TableReservation",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Indoor_BranchId",
                table: "Indoor",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Indoor_OrderId",
                table: "Indoor",
                column: "OrderId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TableReservation_Order_OrderId",
                table: "TableReservation",
                column: "OrderId",
                principalTable: "Order",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
