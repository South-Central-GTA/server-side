using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.DataAccessLayer.Migrations
{
    public partial class AddLicenseModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CharacterLicenses",
                table: "Characters");

            migrationBuilder.CreateTable(
                name: "LicenseModel",
                columns: table => new
                {
                    CharacterModelId = table.Column<int>(type: "integer", nullable: false),
                    CharacterModelId1 = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Warnings = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LicenseModel", x => x.CharacterModelId);
                    table.ForeignKey(
                        name: "FK_LicenseModel_Characters_CharacterModelId1",
                        column: x => x.CharacterModelId1,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LicenseModel_CharacterModelId1",
                table: "LicenseModel",
                column: "CharacterModelId1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LicenseModel");

            migrationBuilder.AddColumn<int>(
                name: "CharacterLicenses",
                table: "Characters",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
