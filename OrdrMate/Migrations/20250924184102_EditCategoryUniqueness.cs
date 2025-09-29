using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrdrMate.Migrations
{
    /// <inheritdoc />
    public partial class EditCategoryUniqueness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Item_Category_CategoryName_PharmacyId",
                table: "Item");

            migrationBuilder.DropTable(
                name: "Category");

            migrationBuilder.DropTable(
                name: "ItemCustomizations");

            migrationBuilder.DropIndex(
                name: "IX_Item_CategoryName_PharmacyId",
                table: "Item");

            migrationBuilder.DropIndex(
                name: "IX_Item_Name_CategoryName_PharmacyId",
                table: "Item");

            migrationBuilder.RenameColumn(
                name: "SubCategoryName",
                table: "Item",
                newName: "SubCategory");

            migrationBuilder.RenameColumn(
                name: "CategoryName",
                table: "Item",
                newName: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Item_Name_Category_SubCategory_PharmacyId",
                table: "Item",
                columns: new[] { "Name", "Category", "SubCategory", "PharmacyId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Item_Name_Category_SubCategory_PharmacyId",
                table: "Item");

            migrationBuilder.RenameColumn(
                name: "SubCategory",
                table: "Item",
                newName: "SubCategoryName");

            migrationBuilder.RenameColumn(
                name: "Category",
                table: "Item",
                newName: "CategoryName");

            migrationBuilder.CreateTable(
                name: "Category",
                columns: table => new
                {
                    Name = table.Column<string>(type: "text", nullable: false),
                    PharmacyId = table.Column<string>(type: "text", nullable: false),
                    Parent = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Category", x => new { x.Name, x.PharmacyId });
                    table.ForeignKey(
                        name: "FK_Category_Category_Parent_PharmacyId",
                        columns: x => new { x.Parent, x.PharmacyId },
                        principalTable: "Category",
                        principalColumns: new[] { "Name", "PharmacyId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Category_Pharmacy_PharmacyId",
                        column: x => x.PharmacyId,
                        principalTable: "Pharmacy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItemCustomizations",
                columns: table => new
                {
                    ItemId = table.Column<string>(type: "text", nullable: false),
                    CategoryId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemCustomizations", x => new { x.ItemId, x.CategoryId });
                    table.ForeignKey(
                        name: "FK_ItemCustomizations_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Item_CategoryName_PharmacyId",
                table: "Item",
                columns: new[] { "CategoryName", "PharmacyId" });

            migrationBuilder.CreateIndex(
                name: "IX_Item_Name_CategoryName_PharmacyId",
                table: "Item",
                columns: new[] { "Name", "CategoryName", "PharmacyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Category_Name_PharmacyId",
                table: "Category",
                columns: new[] { "Name", "PharmacyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Category_Parent_PharmacyId",
                table: "Category",
                columns: new[] { "Parent", "PharmacyId" });

            migrationBuilder.CreateIndex(
                name: "IX_Category_PharmacyId",
                table: "Category",
                column: "PharmacyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Item_Category_CategoryName_PharmacyId",
                table: "Item",
                columns: new[] { "CategoryName", "PharmacyId" },
                principalTable: "Category",
                principalColumns: new[] { "Name", "PharmacyId" },
                onDelete: ReferentialAction.Cascade);
        }
    }
}
