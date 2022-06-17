using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.DataAccessLayer.Migrations
{
    public partial class ImproveClothItem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "DrawableId",
                table: "Items",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GenderType",
                table: "Items",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "TextureId",
                table: "Items",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Items",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DrawableId",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "GenderType",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "TextureId",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Items");
        }
    }
}
