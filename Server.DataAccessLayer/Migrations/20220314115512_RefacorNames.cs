using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.DataAccessLayer.Migrations
{
    public partial class RefacorNames : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_Groups_ItemGroupKeyModel_GroupModelId",
                table: "Items");

            migrationBuilder.DropForeignKey(
                name: "FK_RoleplayInfos_Characters_CharacterModelId",
                table: "RoleplayInfos");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_Characters_OwnerId",
                table: "Vehicles");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "Vehicles",
                newName: "CharacterModelId");

            migrationBuilder.RenameIndex(
                name: "IX_Vehicles_OwnerId",
                table: "Vehicles",
                newName: "IX_Vehicles_CharacterModelId");

            migrationBuilder.RenameColumn(
                name: "Frequence",
                table: "Items",
                newName: "Frequency");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "Houses",
                newName: "CharacterModelId");

            migrationBuilder.AlterColumn<int>(
                name: "CharacterModelId",
                table: "RoleplayInfos",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Groups_ItemGroupKeyModel_GroupModelId",
                table: "Items",
                column: "ItemGroupKeyModel_GroupModelId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RoleplayInfos_Characters_CharacterModelId",
                table: "RoleplayInfos",
                column: "CharacterModelId",
                principalTable: "Characters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_Characters_CharacterModelId",
                table: "Vehicles",
                column: "CharacterModelId",
                principalTable: "Characters",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_Groups_ItemGroupKeyModel_GroupModelId",
                table: "Items");

            migrationBuilder.DropForeignKey(
                name: "FK_RoleplayInfos_Characters_CharacterModelId",
                table: "RoleplayInfos");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_Characters_CharacterModelId",
                table: "Vehicles");

            migrationBuilder.RenameColumn(
                name: "CharacterModelId",
                table: "Vehicles",
                newName: "OwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_Vehicles_CharacterModelId",
                table: "Vehicles",
                newName: "IX_Vehicles_OwnerId");

            migrationBuilder.RenameColumn(
                name: "Frequency",
                table: "Items",
                newName: "Frequence");

            migrationBuilder.RenameColumn(
                name: "CharacterModelId",
                table: "Houses",
                newName: "OwnerId");

            migrationBuilder.AlterColumn<int>(
                name: "CharacterModelId",
                table: "RoleplayInfos",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Groups_ItemGroupKeyModel_GroupModelId",
                table: "Items",
                column: "ItemGroupKeyModel_GroupModelId",
                principalTable: "Groups",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RoleplayInfos_Characters_CharacterModelId",
                table: "RoleplayInfos",
                column: "CharacterModelId",
                principalTable: "Characters",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_Characters_OwnerId",
                table: "Vehicles",
                column: "OwnerId",
                principalTable: "Characters",
                principalColumn: "Id");
        }
    }
}
