using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrdrMate.Migrations
{
    /// <inheritdoc />
    public partial class DropTableReservationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TableReservation");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TableReservation",
                columns: table => new
                {
                    ReservationId = table.Column<string>(type: "text", nullable: false),
                    BranchId = table.Column<string>(type: "text", nullable: false),
                    CustomerId = table.Column<string>(type: "text", nullable: false),
                    OrderId = table.Column<string>(type: "text", nullable: false),
                    ReservationStatus = table.Column<string>(type: "text", nullable: false),
                    ReservationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TableNumber = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableReservation", x => x.ReservationId);
                    table.ForeignKey(
                        name: "FK_TableReservation_Branch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TableReservation_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TableReservation_User_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TableReservation_BranchId",
                table: "TableReservation",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_TableReservation_CustomerId",
                table: "TableReservation",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_TableReservation_OrderId",
                table: "TableReservation",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TableReservation_TableNumber_BranchId_ReservationTime",
                table: "TableReservation",
                columns: new[] { "TableNumber", "BranchId", "ReservationTime" },
                unique: true);
        }
    }
}
