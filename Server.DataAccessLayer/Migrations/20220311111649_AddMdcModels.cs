using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Server.DataAccessLayer.Migrations
{
    public partial class AddMdcModels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CriminalRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CharacterModelId = table.Column<int>(type: "integer", nullable: true),
                    CreatorCharacterName = table.Column<string>(type: "text", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsage = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CriminalRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CriminalRecords_Characters_CharacterModelId",
                        column: x => x.CharacterModelId,
                        principalTable: "Characters",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MdcNodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TargetModelId = table.Column<int>(type: "integer", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    CreatorCharacterName = table.Column<string>(type: "text", nullable: false),
                    Node = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsage = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MdcNodes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CriminalRecords_CharacterModelId",
                table: "CriminalRecords",
                column: "CharacterModelId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CriminalRecords");

            migrationBuilder.DropTable(
                name: "MdcNodes");
        }
    }
}
