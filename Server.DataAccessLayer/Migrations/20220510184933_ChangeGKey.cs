using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.DataAccessLayer.Migrations
{
    public partial class ChangeGKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_Groups_ItemGroupKeyModel_GroupModelId",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_ItemGroupKeyModel_GroupModelId",
                table: "Items");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Items_ItemGroupKeyModel_GroupModelId",
                table: "Items",
                column: "ItemGroupKeyModel_GroupModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Groups_ItemGroupKeyModel_GroupModelId",
                table: "Items",
                column: "ItemGroupKeyModel_GroupModelId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
