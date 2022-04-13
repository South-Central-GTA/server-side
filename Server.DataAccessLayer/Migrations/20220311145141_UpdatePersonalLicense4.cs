using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Server.DataAccessLayer.Migrations
{
    public partial class UpdatePersonalLicense4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PersonalLicenses_Characters_CharacterModelId1",
                table: "PersonalLicenses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PersonalLicenses",
                table: "PersonalLicenses");

            migrationBuilder.DropIndex(
                name: "IX_PersonalLicenses_CharacterModelId1",
                table: "PersonalLicenses");

            migrationBuilder.RenameColumn(
                name: "CharacterModelId1",
                table: "PersonalLicenses",
                newName: "Id");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "PersonalLicenses",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PersonalLicenses",
                table: "PersonalLicenses",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalLicenses_CharacterModelId",
                table: "PersonalLicenses",
                column: "CharacterModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_PersonalLicenses_Characters_CharacterModelId",
                table: "PersonalLicenses",
                column: "CharacterModelId",
                principalTable: "Characters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PersonalLicenses_Characters_CharacterModelId",
                table: "PersonalLicenses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PersonalLicenses",
                table: "PersonalLicenses");

            migrationBuilder.DropIndex(
                name: "IX_PersonalLicenses_CharacterModelId",
                table: "PersonalLicenses");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "PersonalLicenses",
                newName: "CharacterModelId1");

            migrationBuilder.AlterColumn<int>(
                name: "CharacterModelId1",
                table: "PersonalLicenses",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PersonalLicenses",
                table: "PersonalLicenses",
                column: "CharacterModelId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalLicenses_CharacterModelId1",
                table: "PersonalLicenses",
                column: "CharacterModelId1");

            migrationBuilder.AddForeignKey(
                name: "FK_PersonalLicenses_Characters_CharacterModelId1",
                table: "PersonalLicenses",
                column: "CharacterModelId1",
                principalTable: "Characters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
