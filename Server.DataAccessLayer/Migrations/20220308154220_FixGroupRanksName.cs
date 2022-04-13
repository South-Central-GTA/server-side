using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.DataAccessLayer.Migrations
{
    public partial class FixGroupRanksName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupRankModel_Groups_GroupModelId",
                table: "GroupRankModel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GroupRankModel",
                table: "GroupRankModel");

            migrationBuilder.RenameTable(
                name: "GroupRankModel",
                newName: "GroupRanks");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GroupRanks",
                table: "GroupRanks",
                columns: new[] { "GroupModelId", "Level" });

            migrationBuilder.AddForeignKey(
                name: "FK_GroupRanks_Groups_GroupModelId",
                table: "GroupRanks",
                column: "GroupModelId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupRanks_Groups_GroupModelId",
                table: "GroupRanks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GroupRanks",
                table: "GroupRanks");

            migrationBuilder.RenameTable(
                name: "GroupRanks",
                newName: "GroupRankModel");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GroupRankModel",
                table: "GroupRankModel",
                columns: new[] { "GroupModelId", "Level" });

            migrationBuilder.AddForeignKey(
                name: "FK_GroupRankModel_Groups_GroupModelId",
                table: "GroupRankModel",
                column: "GroupModelId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
