using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Server.DataAccessLayer.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    SocialClubId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    DiscordId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    CurrentName = table.Column<string>(type: "text", nullable: false),
                    NameHistory = table.Column<List<string>>(type: "text[]", nullable: false),
                    HardwareIdHash = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    HardwareIdExHash = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    Permission = table.Column<int>(type: "integer", nullable: false),
                    BannedFrom = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    BannedReason = table.Column<string>(type: "text", nullable: true),
                    BannedPermanent = table.Column<bool>(type: "boolean", nullable: false),
                    BannedUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastLogin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OnlineSince = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AdminCheckpoints = table.Column<int>(type: "integer", nullable: false),
                    SouthCentralPoints = table.Column<int>(type: "integer", nullable: false),
                    LastIp = table.Column<string>(type: "text", nullable: false),
                    LastSelectedCharacterId = table.Column<int>(type: "integer", nullable: false),
                    MaxCharacters = table.Column<int>(type: "integer", nullable: false),
                    MaxAnimations = table.Column<int>(type: "integer", nullable: false),
                    MaxRoleplayInfos = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsage = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.SocialClubId);
                });

            migrationBuilder.CreateTable(
                name: "Animations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Dictionary = table.Column<string>(type: "text", nullable: false),
                    Clip = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Flags = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsage = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Animations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BankAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<long>(type: "bigint", nullable: false),
                    BankDetails = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsage = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmergencyCalls",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PhoneNumber = table.Column<string>(type: "text", nullable: false),
                    Location = table.Column<string>(type: "text", nullable: false),
                    Situation = table.Column<string>(type: "text", nullable: false),
                    FactionType = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsage = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmergencyCalls", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    GroupType = table.Column<int>(type: "integer", nullable: false),
                    MaxRanks = table.Column<int>(type: "integer", nullable: false),
                    LicensesFlags = table.Column<int>(type: "integer", nullable: true),
                    PurchasedLicenses = table.Column<int>(type: "integer", nullable: true),
                    Products = table.Column<int>(type: "integer", nullable: true),
                    DeliveryVisibilityStatus = table.Column<int>(type: "integer", nullable: true),
                    FactionType = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsage = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItemCatalog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Model = table.Column<string>(type: "text", nullable: false),
                    ZOffset = table.Column<float>(type: "real", nullable: false),
                    Image = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Rarity = table.Column<int>(type: "integer", nullable: false),
                    Weight = table.Column<float>(type: "real", nullable: false),
                    Equippable = table.Column<bool>(type: "boolean", nullable: false),
                    Stackable = table.Column<bool>(type: "boolean", nullable: false),
                    Buyable = table.Column<bool>(type: "boolean", nullable: false),
                    Sellable = table.Column<bool>(type: "boolean", nullable: false),
                    Price = table.Column<int>(type: "integer", nullable: false),
                    SellPrice = table.Column<int>(type: "integer", nullable: false),
                    MaxLimit = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsage = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PositionX = table.Column<float>(type: "real", nullable: false),
                    PositionY = table.Column<float>(type: "real", nullable: false),
                    PositionZ = table.Column<float>(type: "real", nullable: false),
                    Roll = table.Column<float>(type: "real", nullable: false),
                    Pitch = table.Column<float>(type: "real", nullable: false),
                    Yaw = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemCatalog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MailAccounts",
                columns: table => new
                {
                    MailAddress = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsage = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MailAccounts", x => x.MailAddress);
                });

            migrationBuilder.CreateTable(
                name: "Mails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SenderMailAddress = table.Column<string>(type: "text", nullable: false),
                    MailReadedFromAddress = table.Column<List<string>>(type: "text[]", nullable: false),
                    Title = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Context = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsage = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MdcNodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TargetModelId = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    CreatorCharacterName = table.Column<string>(type: "text", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsage = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MdcNodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserShopDatas",
                columns: table => new
                {
                    CharacterModelId = table.Column<int>(type: "integer", nullable: false),
                    GotWarned = table.Column<bool>(type: "boolean", nullable: false),
                    BillToPay = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsage = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserShopDatas", x => x.CharacterModelId);
                });

            migrationBuilder.CreateTable(
                name: "VehicleCatalog",
                columns: table => new
                {
                    Model = table.Column<string>(type: "text", nullable: false),
                    DisplayName = table.Column<string>(type: "text", nullable: false),
                    DisplayClass = table.Column<string>(type: "text", nullable: false),
                    ClassId = table.Column<string>(type: "text", nullable: false),
                    MaxTank = table.Column<int>(type: "integer", nullable: false),
                    FuelType = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<int>(type: "integer", nullable: false),
                    DlcName = table.Column<string>(type: "text", nullable: false),
                    AmountOfOrderableVehicles = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsage = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleCatalog", x => x.Model);
                });

            migrationBuilder.CreateTable(
                name: "Characters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccountModelId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    OnlineSince = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    Age = table.Column<int>(type: "integer", nullable: false),
                    Origin = table.Column<string>(type: "text", nullable: false),
                    Physique = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Story = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    BodySize = table.Column<int>(type: "integer", nullable: false),
                    Gender = table.Column<int>(type: "integer", nullable: false),
                    Mother = table.Column<int>(type: "integer", nullable: false),
                    Father = table.Column<int>(type: "integer", nullable: false),
                    Similarity = table.Column<float>(type: "real", nullable: false),
                    SkinSimilarity = table.Column<float>(type: "real", nullable: false),
                    CharacterState = table.Column<int>(type: "integer", nullable: false),
                    Torso = table.Column<int>(type: "integer", nullable: false),
                    TorsoTexture = table.Column<int>(type: "integer", nullable: false),
                    Health = table.Column<int>(type: "integer", nullable: false),
                    Armor = table.Column<int>(type: "integer", nullable: false),
                    DeathState = table.Column<int>(type: "integer", nullable: false),
                    AnimationIds = table.Column<List<int>>(type: "integer[]", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsage = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PositionX = table.Column<float>(type: "real", nullable: false),
                    PositionY = table.Column<float>(type: "real", nullable: false),
                    PositionZ = table.Column<float>(type: "real", nullable: false),
                    Roll = table.Column<float>(type: "real", nullable: false),
                    Pitch = table.Column<float>(type: "real", nullable: false),
                    Yaw = table.Column<float>(type: "real", nullable: false),
                    Dimension = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Characters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Characters_Accounts_AccountModelId",
                        column: x => x.AccountModelId,
                        principalTable: "Accounts",
                        principalColumn: "SocialClubId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BankHistoryEntryModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BankAccountModelId = table.Column<int>(type: "integer", nullable: false),
                    HistoryType = table.Column<int>(type: "integer", nullable: false),
                    Income = table.Column<bool>(type: "boolean", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false),
                    PurposeOfUse = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsage = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankHistoryEntryModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankHistoryEntryModel_BankAccounts_BankAccountModelId",
                        column: x => x.BankAccountModelId,
                        principalTable: "BankAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BankAccountGroupAccesses",
                columns: table => new
                {
                    BankAccountModelId = table.Column<int>(type: "integer", nullable: false),
                    GroupModelId = table.Column<int>(type: "integer", nullable: false),
                    Owner = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankAccountGroupAccesses", x => new { x.BankAccountModelId, x.GroupModelId });
                    table.ForeignKey(
                        name: "FK_BankAccountGroupAccesses_BankAccounts_BankAccountModelId",
                        column: x => x.BankAccountModelId,
                        principalTable: "BankAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BankAccountGroupAccesses_Groups_GroupModelId",
                        column: x => x.GroupModelId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Directories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GroupModelId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ReadGroupLevel = table.Column<int>(type: "integer", nullable: false),
                    WriteGroupLevel = table.Column<int>(type: "integer", nullable: false),
                    LastEditCharacterName = table.Column<string>(type: "text", nullable: false),
                    CreatorCharacterId = table.Column<int>(type: "integer", nullable: false),
                    CreatorCharacterName = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsage = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Directories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Directories_Groups_GroupModelId",
                        column: x => x.GroupModelId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupRanks",
                columns: table => new
                {
                    GroupModelId = table.Column<int>(type: "integer", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    GroupPermission = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsage = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupRanks", x => new { x.GroupModelId, x.Level });
                    table.ForeignKey(
                        name: "FK_GroupRanks_Groups_GroupModelId",
                        column: x => x.GroupModelId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Houses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CharacterModelId = table.Column<int>(type: "integer", nullable: true),
                    GroupModelId = table.Column<int>(type: "integer", nullable: true),
                    HouseType = table.Column<int>(type: "integer", nullable: false),
                    HouseNumber = table.Column<int>(type: "integer", nullable: false),
                    SubName = table.Column<string>(type: "text", nullable: false),
                    StreetDirection = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<int>(type: "integer", nullable: false),
                    InteriorId = table.Column<int>(type: "integer", nullable: true),
                    Rentable = table.Column<bool>(type: "boolean", nullable: false),
                    BlockedOwnership = table.Column<bool>(type: "boolean", nullable: false),
                    RentBankAccountId = table.Column<int>(type: "integer", nullable: true),
                    LockState = table.Column<int>(type: "integer", nullable: false),
                    Keys = table.Column<List<int>>(type: "integer[]", nullable: false),
                    LeaseCompanyType = table.Column<int>(type: "integer", nullable: true),
                    HasCashier = table.Column<bool>(type: "boolean", nullable: true),
                    PlayerDuties = table.Column<int>(type: "integer", nullable: true),
                    CashierX = table.Column<float>(type: "real", nullable: true),
                    CashierY = table.Column<float>(type: "real", nullable: true),
                    CashierZ = table.Column<float>(type: "real", nullable: true),
                    CashierHeading = table.Column<float>(type: "real", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsage = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PositionX = table.Column<float>(type: "real", nullable: false),
                    PositionY = table.Column<float>(type: "real", nullable: false),
                    PositionZ = table.Column<float>(type: "real", nullable: false),
                    Roll = table.Column<float>(type: "real", nullable: false),
                    Pitch = table.Column<float>(type: "real", nullable: false),
                    Yaw = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Houses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Houses_Groups_GroupModelId",
                        column: x => x.GroupModelId,
                        principalTable: "Groups",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MailAccountGroupAccessModel",
                columns: table => new
                {
                    MailAccountModelMailAddress = table.Column<string>(type: "text", nullable: false),
                    GroupModelId = table.Column<int>(type: "integer", nullable: false),
                    Owner = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MailAccountGroupAccessModel", x => new { x.MailAccountModelMailAddress, x.GroupModelId });
                    table.ForeignKey(
                        name: "FK_MailAccountGroupAccessModel_Groups_GroupModelId",
                        column: x => x.GroupModelId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MailAccountGroupAccessModel_MailAccounts_MailAccountModelMa~",
                        column: x => x.MailAccountModelMailAddress,
                        principalTable: "MailAccounts",
                        principalColumn: "MailAddress",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MailLinkModel",
                columns: table => new
                {
                    MailAccountModelMailAddress = table.Column<string>(type: "text", nullable: false),
                    MailModelId = table.Column<int>(type: "integer", nullable: false),
                    IsAuthor = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MailLinkModel", x => new { x.MailAccountModelMailAddress, x.MailModelId });
                    table.ForeignKey(
                        name: "FK_MailLinkModel_MailAccounts_MailAccountModelMailAddress",
                        column: x => x.MailAccountModelMailAddress,
                        principalTable: "MailAccounts",
                        principalColumn: "MailAddress",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MailLinkModel_Mails_MailModelId",
                        column: x => x.MailModelId,
                        principalTable: "Mails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderedVehicles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderedBy = table.Column<string>(type: "text", nullable: false),
                    CatalogVehicleModelId = table.Column<string>(type: "text", nullable: false),
                    GroupModelId = table.Column<int>(type: "integer", nullable: false),
                    DeliverdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeliveryRequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeliveryRequestedBy = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsage = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderedVehicles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderedVehicles_VehicleCatalog_CatalogVehicleModelId",
                        column: x => x.CatalogVehicleModelId,
                        principalTable: "VehicleCatalog",
                        principalColumn: "Model",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Appearances",
                columns: table => new
                {
                    CharacterModelId = table.Column<int>(type: "integer", nullable: false),
                    Hair = table.Column<int>(type: "integer", nullable: false),
                    PrimHairColor = table.Column<int>(type: "integer", nullable: false),
                    SecHairColor = table.Column<int>(type: "integer", nullable: false),
                    EyeColor = table.Column<int>(type: "integer", nullable: false),
                    BlemishesValue = table.Column<int>(type: "integer", nullable: false),
                    BlemishesOpacity = table.Column<float>(type: "real", nullable: false),
                    BlemishesColor = table.Column<int>(type: "integer", nullable: false),
                    FacialhairValue = table.Column<int>(type: "integer", nullable: false),
                    FacialhairOpacity = table.Column<float>(type: "real", nullable: false),
                    FacialhairColor = table.Column<int>(type: "integer", nullable: false),
                    EyebrowsValue = table.Column<int>(type: "integer", nullable: false),
                    EyebrowsOpacity = table.Column<float>(type: "real", nullable: false),
                    EyebrowsColor = table.Column<int>(type: "integer", nullable: false),
                    AgeingValue = table.Column<int>(type: "integer", nullable: false),
                    AgeingOpacity = table.Column<float>(type: "real", nullable: false),
                    AgeingColor = table.Column<int>(type: "integer", nullable: false),
                    MakeupValue = table.Column<int>(type: "integer", nullable: false),
                    MakeupOpacity = table.Column<float>(type: "real", nullable: false),
                    MakeupColor = table.Column<int>(type: "integer", nullable: false),
                    BlushValue = table.Column<int>(type: "integer", nullable: false),
                    BlushOpacity = table.Column<float>(type: "real", nullable: false),
                    BlushColor = table.Column<int>(type: "integer", nullable: false),
                    ComplexionValue = table.Column<int>(type: "integer", nullable: false),
                    ComplexionOpacity = table.Column<float>(type: "real", nullable: false),
                    ComplexionColor = table.Column<int>(type: "integer", nullable: false),
                    SundamageValue = table.Column<int>(type: "integer", nullable: false),
                    SundamageOpacity = table.Column<float>(type: "real", nullable: false),
                    SundamageColor = table.Column<int>(type: "integer", nullable: false),
                    LipstickValue = table.Column<int>(type: "integer", nullable: false),
                    LipstickOpacity = table.Column<float>(type: "real", nullable: false),
                    LipstickColor = table.Column<int>(type: "integer", nullable: false),
                    FrecklesValue = table.Column<int>(type: "integer", nullable: false),
                    FrecklesOpacity = table.Column<float>(type: "real", nullable: false),
                    FrecklesColor = table.Column<int>(type: "integer", nullable: false),
                    ChesthairValue = table.Column<int>(type: "integer", nullable: false),
                    ChesthairOpacity = table.Column<float>(type: "real", nullable: false),
                    ChesthairColor = table.Column<int>(type: "integer", nullable: false),
                    BodyblemishesValue = table.Column<int>(type: "integer", nullable: false),
                    BodyblemishesOpacity = table.Column<float>(type: "real", nullable: false),
                    BodyblemishesColor = table.Column<int>(type: "integer", nullable: false),
                    AddbodyblemihesValue = table.Column<int>(type: "integer", nullable: false),
                    AddbodyblemihesOpacity = table.Column<float>(type: "real", nullable: false),
                    AddbodyblemihesColor = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsage = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appearances", x => x.CharacterModelId);
                    table.ForeignKey(
                        name: "FK_Appearances_Characters_CharacterModelId",
                        column: x => x.CharacterModelId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BankAccountCharacterAccesses",
                columns: table => new
                {
                    BankAccountModelId = table.Column<int>(type: "integer", nullable: false),
                    CharacterModelId = table.Column<int>(type: "integer", nullable: false),
                    Permission = table.Column<int>(type: "integer", nullable: false),
                    Owner = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsage = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankAccountCharacterAccesses", x => new { x.BankAccountModelId, x.CharacterModelId });
                    table.ForeignKey(
                        name: "FK_BankAccountCharacterAccesses_BankAccounts_BankAccountModelId",
                        column: x => x.BankAccountModelId,
                        principalTable: "BankAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BankAccountCharacterAccesses_Characters_CharacterModelId",
                        column: x => x.CharacterModelId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChatLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccountModelId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    CharacterModelId = table.Column<int>(type: "integer", nullable: false),
                    ChatType = table.Column<int>(type: "integer", nullable: false),
                    Text = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    LoggedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsage = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatLogs_Accounts_AccountModelId",
                        column: x => x.AccountModelId,
                        principalTable: "Accounts",
                        principalColumn: "SocialClubId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChatLogs_Characters_CharacterModelId",
                        column: x => x.CharacterModelId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CommandLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccountModelId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    CharacterModelId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    Arguments = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    LoggedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RequiredPermission = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsage = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommandLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommandLogs_Accounts_AccountModelId",
                        column: x => x.AccountModelId,
                        principalTable: "Accounts",
                        principalColumn: "SocialClubId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommandLogs_Characters_CharacterModelId",
                        column: x => x.CharacterModelId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CriminalRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CharacterModelId = table.Column<int>(type: "integer", nullable: false),
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
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DefinedJobs",
                columns: table => new
                {
                    CharacterModelId = table.Column<int>(type: "integer", nullable: false),
                    JobId = table.Column<int>(type: "integer", nullable: false),
                    BankAccountId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsage = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DefinedJobs", x => x.CharacterModelId);
                    table.ForeignKey(
                        name: "FK_DefinedJobs_Characters_CharacterModelId",
                        column: x => x.CharacterModelId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FaceFeatures",
                columns: table => new
                {
                    CharacterModelId = table.Column<int>(type: "integer", nullable: false),
                    EyesSize = table.Column<float>(type: "real", nullable: false),
                    LipsThickness = table.Column<float>(type: "real", nullable: false),
                    NoseWidth = table.Column<float>(type: "real", nullable: false),
                    NoseHeight = table.Column<float>(type: "real", nullable: false),
                    NoseLength = table.Column<float>(type: "real", nullable: false),
                    NoseBridge = table.Column<float>(type: "real", nullable: false),
                    NoseTip = table.Column<float>(type: "real", nullable: false),
                    NoseBridgeShift = table.Column<float>(type: "real", nullable: false),
                    BrowHeight = table.Column<float>(type: "real", nullable: false),
                    BrowWidth = table.Column<float>(type: "real", nullable: false),
                    CheekboneHeight = table.Column<float>(type: "real", nullable: false),
                    CheekboneWidth = table.Column<float>(type: "real", nullable: false),
                    CheekWidth = table.Column<float>(type: "real", nullable: false),
                    JawWidth = table.Column<float>(type: "real", nullable: false),
                    JawHeight = table.Column<float>(type: "real", nullable: false),
                    ChinLength = table.Column<float>(type: "real", nullable: false),
                    ChinPosition = table.Column<float>(type: "real", nullable: false),
                    ChinWidth = table.Column<float>(type: "real", nullable: false),
                    ChinShape = table.Column<float>(type: "real", nullable: false),
                    NeckWidth = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaceFeatures", x => x.CharacterModelId);
                    table.ForeignKey(
                        name: "FK_FaceFeatures_Characters_CharacterModelId",
                        column: x => x.CharacterModelId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupMembers",
                columns: table => new
                {
                    GroupModelId = table.Column<int>(type: "integer", nullable: false),
                    CharacterModelId = table.Column<int>(type: "integer", nullable: false),
                    RankLevel = table.Column<long>(type: "bigint", nullable: false),
                    Salary = table.Column<long>(type: "bigint", nullable: false),
                    BankAccountId = table.Column<int>(type: "integer", nullable: false),
                    Owner = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsage = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupMembers", x => new { x.GroupModelId, x.CharacterModelId });
                    table.ForeignKey(
                        name: "FK_GroupMembers_Characters_CharacterModelId",
                        column: x => x.CharacterModelId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupMembers_Groups_GroupModelId",
                        column: x => x.GroupModelId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MailAccountCharacterAccessModel",
                columns: table => new
                {
                    MailAccountModelMailAddress = table.Column<string>(type: "text", nullable: false),
                    CharacterModelId = table.Column<int>(type: "integer", nullable: false),
                    Permission = table.Column<int>(type: "integer", nullable: false),
                    Owner = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsage = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MailAccountCharacterAccessModel", x => new { x.MailAccountModelMailAddress, x.CharacterModelId });
                    table.ForeignKey(
                        name: "FK_MailAccountCharacterAccessModel_Characters_CharacterModelId",
                        column: x => x.CharacterModelId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MailAccountCharacterAccessModel_MailAccounts_MailAccountMod~",
                        column: x => x.MailAccountModelMailAddress,
                        principalTable: "MailAccounts",
                        principalColumn: "MailAddress",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MdcAllergies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CharacterModelId = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    CreatorCharacterName = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsage = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MdcAllergies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MdcAllergies_Characters_CharacterModelId",
                        column: x => x.CharacterModelId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MdcMedicalEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CharacterModelId = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    CreatorCharacterName = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsage = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MdcMedicalEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MdcMedicalEntries_Characters_CharacterModelId",
                        column: x => x.CharacterModelId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PersonalLicenses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CharacterModelId = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Warnings = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsage = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalLicenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonalLicenses_Characters_CharacterModelId",
                        column: x => x.CharacterModelId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RegistrationOfficeEntries",
                columns: table => new
                {
                    CharacterModelId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsage = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrationOfficeEntries", x => x.CharacterModelId);
                    table.ForeignKey(
                        name: "FK_RegistrationOfficeEntries_Characters_CharacterModelId",
                        column: x => x.CharacterModelId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoleplayInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MarkerId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    CharacterModelId = table.Column<int>(type: "integer", nullable: false),
                    Dimension = table.Column<int>(type: "integer", nullable: false),
                    Distance = table.Column<int>(type: "integer", nullable: false),
                    Context = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsage = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PositionX = table.Column<float>(type: "real", nullable: false),
                    PositionY = table.Column<float>(type: "real", nullable: false),
                    PositionZ = table.Column<float>(type: "real", nullable: false),
                    Roll = table.Column<float>(type: "real", nullable: false),
                    Pitch = table.Column<float>(type: "real", nullable: false),
                    Yaw = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleplayInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleplayInfos_Characters_CharacterModelId",
                        column: x => x.CharacterModelId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TattoosModel",
                columns: table => new
                {
                    CharacterModelId = table.Column<int>(type: "integer", nullable: false),
                    HeadCollection = table.Column<string>(type: "text", nullable: false),
                    HeadHash = table.Column<string>(type: "text", nullable: false),
                    TorsoCollection = table.Column<string>(type: "text", nullable: false),
                    TorsoHash = table.Column<string>(type: "text", nullable: false),
                    LeftArmCollection = table.Column<string>(type: "text", nullable: false),
                    LeftArmHash = table.Column<string>(type: "text", nullable: false),
                    RightArmCollection = table.Column<string>(type: "text", nullable: false),
                    RightArmHash = table.Column<string>(type: "text", nullable: false),
                    LeftLegCollection = table.Column<string>(type: "text", nullable: false),
                    LeftLegHash = table.Column<string>(type: "text", nullable: false),
                    RightLegCollection = table.Column<string>(type: "text", nullable: false),
                    RightLegHash = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsage = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TattoosModel", x => x.CharacterModelId);
                    table.ForeignKey(
                        name: "FK_TattoosModel_Characters_CharacterModelId",
                        column: x => x.CharacterModelId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRecordLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccountModelId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    StaffAccountModelId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    CharacterModelId = table.Column<int>(type: "integer", nullable: true),
                    UserRecordType = table.Column<int>(type: "integer", nullable: false),
                    Text = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    LoggedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsage = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRecordLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRecordLogs_Accounts_AccountModelId",
                        column: x => x.AccountModelId,
                        principalTable: "Accounts",
                        principalColumn: "SocialClubId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRecordLogs_Accounts_StaffAccountModelId",
                        column: x => x.StaffAccountModelId,
                        principalTable: "Accounts",
                        principalColumn: "SocialClubId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRecordLogs_Characters_CharacterModelId",
                        column: x => x.CharacterModelId,
                        principalTable: "Characters",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Vehicles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CharacterModelId = table.Column<int>(type: "integer", nullable: true),
                    GroupModelOwnerId = table.Column<int>(type: "integer", nullable: true),
                    Model = table.Column<string>(type: "text", nullable: false),
                    VehicleState = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<int>(type: "integer", nullable: false),
                    NumberplateText = table.Column<string>(type: "text", nullable: false),
                    EngineHealth = table.Column<int>(type: "integer", nullable: false),
                    BodyHealth = table.Column<long>(type: "bigint", nullable: false),
                    PrimaryColor = table.Column<int>(type: "integer", nullable: false),
                    SecondaryColor = table.Column<int>(type: "integer", nullable: false),
                    Livery = table.Column<byte>(type: "smallint", nullable: false),
                    Fuel = table.Column<float>(type: "real", nullable: false),
                    DrivenKilometre = table.Column<float>(type: "real", nullable: false),
                    LastDrivers = table.Column<List<string>>(type: "text[]", nullable: false),
                    EngineOn = table.Column<bool>(type: "boolean", nullable: false),
                    LockState = table.Column<int>(type: "integer", nullable: false),
                    Keys = table.Column<List<int>>(type: "integer[]", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsage = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PositionX = table.Column<float>(type: "real", nullable: false),
                    PositionY = table.Column<float>(type: "real", nullable: false),
                    PositionZ = table.Column<float>(type: "real", nullable: false),
                    Roll = table.Column<float>(type: "real", nullable: false),
                    Pitch = table.Column<float>(type: "real", nullable: false),
                    Yaw = table.Column<float>(type: "real", nullable: false),
                    Dimension = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vehicles_Characters_CharacterModelId",
                        column: x => x.CharacterModelId,
                        principalTable: "Characters",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Vehicles_Groups_GroupModelOwnerId",
                        column: x => x.GroupModelOwnerId,
                        principalTable: "Groups",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Files",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DirectoryModelId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Context = table.Column<string>(type: "text", nullable: false),
                    IsBlocked = table.Column<bool>(type: "boolean", nullable: false),
                    BlockedByCharacterName = table.Column<string>(type: "text", nullable: true),
                    LastEditCharacterName = table.Column<string>(type: "text", nullable: false),
                    CreatorCharacterId = table.Column<int>(type: "integer", nullable: false),
                    CreatorCharacterName = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsage = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Files", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Files_Directories_DirectoryModelId",
                        column: x => x.DirectoryModelId,
                        principalTable: "Directories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Doors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HouseModelId = table.Column<int>(type: "integer", nullable: false),
                    Hash = table.Column<long>(type: "bigint", nullable: false),
                    LockState = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsage = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PositionX = table.Column<float>(type: "real", nullable: false),
                    PositionY = table.Column<float>(type: "real", nullable: false),
                    PositionZ = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Doors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Doors_Houses_HouseModelId",
                        column: x => x.HouseModelId,
                        principalTable: "Houses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Deliveries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DeliveryType = table.Column<int>(type: "integer", nullable: false),
                    OrderGroupModelId = table.Column<int>(type: "integer", nullable: false),
                    SupplierGroupModelId = table.Column<int>(type: "integer", nullable: true),
                    SupplierCharacterId = table.Column<int>(type: "integer", nullable: true),
                    SupplierPhoneNumber = table.Column<string>(type: "text", nullable: true),
                    SupplierFullName = table.Column<string>(type: "text", nullable: true),
                    PlayerVehicleModelId = table.Column<int>(type: "integer", nullable: true),
                    OldStatus = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ProductsRemaining = table.Column<int>(type: "integer", nullable: true),
                    OrderedProducts = table.Column<int>(type: "integer", nullable: true),
                    VehicleModel = table.Column<string>(type: "text", nullable: true),
                    DisplayName = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsage = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deliveries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Deliveries_Groups_OrderGroupModelId",
                        column: x => x.OrderGroupModelId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Deliveries_Groups_SupplierGroupModelId",
                        column: x => x.SupplierGroupModelId,
                        principalTable: "Groups",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Deliveries_Vehicles_PlayerVehicleModelId",
                        column: x => x.PlayerVehicleModelId,
                        principalTable: "Vehicles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PublicGarageEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GroupModelId = table.Column<int>(type: "integer", nullable: true),
                    CharacterModelId = table.Column<int>(type: "integer", nullable: true),
                    PlayerVehicleModelId = table.Column<int>(type: "integer", nullable: false),
                    GarageId = table.Column<int>(type: "integer", nullable: false),
                    BankAccountId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsage = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicGarageEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PublicGarageEntries_Characters_CharacterModelId",
                        column: x => x.CharacterModelId,
                        principalTable: "Characters",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PublicGarageEntries_Groups_GroupModelId",
                        column: x => x.GroupModelId,
                        principalTable: "Groups",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PublicGarageEntries_Vehicles_PlayerVehicleModelId",
                        column: x => x.PlayerVehicleModelId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Inventories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CharacterModelId = table.Column<int>(type: "integer", nullable: true),
                    HouseModelId = table.Column<int>(type: "integer", nullable: true),
                    VehicleModelId = table.Column<int>(type: "integer", nullable: true),
                    ItemClothModelId = table.Column<int>(type: "integer", nullable: true),
                    GroupCharacterId = table.Column<int>(type: "integer", nullable: true),
                    GroupId = table.Column<int>(type: "integer", nullable: true),
                    InventoryType = table.Column<int>(type: "integer", nullable: false),
                    MaxWeight = table.Column<float>(type: "real", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsage = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inventories_Characters_CharacterModelId",
                        column: x => x.CharacterModelId,
                        principalTable: "Characters",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Inventories_Houses_HouseModelId",
                        column: x => x.HouseModelId,
                        principalTable: "Houses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Inventories_Vehicles_VehicleModelId",
                        column: x => x.VehicleModelId,
                        principalTable: "Vehicles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InventoryModelId = table.Column<int>(type: "integer", nullable: true),
                    CatalogItemModelId = table.Column<int>(type: "integer", nullable: false),
                    ItemType = table.Column<int>(type: "integer", nullable: false),
                    Slot = table.Column<int>(type: "integer", nullable: true),
                    DroppedByCharacter = table.Column<string>(type: "text", nullable: true),
                    CustomData = table.Column<string>(type: "text", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    Amount = table.Column<int>(type: "integer", nullable: false),
                    Condition = table.Column<int>(type: "integer", nullable: true),
                    IsBought = table.Column<bool>(type: "boolean", nullable: false),
                    IsStolen = table.Column<bool>(type: "boolean", nullable: false),
                    ItemState = table.Column<int>(type: "integer", nullable: false),
                    ItemGroupKeyModel_GroupModelId = table.Column<int>(type: "integer", nullable: true),
                    GroupModelId = table.Column<int>(type: "integer", nullable: true),
                    ItemKeyModelId = table.Column<int>(type: "integer", nullable: true),
                    HouseModelId = table.Column<int>(type: "integer", nullable: true),
                    PlayerVehicleModelId = table.Column<int>(type: "integer", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    Active = table.Column<bool>(type: "boolean", nullable: true),
                    BackgroundImageId = table.Column<int>(type: "integer", nullable: true),
                    CurrentOwnerId = table.Column<int>(type: "integer", nullable: true),
                    InitialOwnerId = table.Column<int>(type: "integer", nullable: true),
                    LastTimeOpenedNotifications = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FactionType = table.Column<int>(type: "integer", nullable: true),
                    Frequency = table.Column<int>(type: "integer", nullable: true),
                    ItemWeaponId = table.Column<int>(type: "integer", nullable: true),
                    ItemModelWeaponId = table.Column<int>(type: "integer", nullable: true),
                    SerialNumber = table.Column<string>(type: "text", nullable: true),
                    ItemWeaponModel_InitialOwnerId = table.Column<int>(type: "integer", nullable: true),
                    ComponentHashes = table.Column<List<string>>(type: "text[]", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsage = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PositionX = table.Column<float>(type: "real", nullable: false),
                    PositionY = table.Column<float>(type: "real", nullable: false),
                    PositionZ = table.Column<float>(type: "real", nullable: false),
                    Roll = table.Column<float>(type: "real", nullable: false),
                    Pitch = table.Column<float>(type: "real", nullable: false),
                    Yaw = table.Column<float>(type: "real", nullable: false),
                    Dimension = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Items_Groups_ItemGroupKeyModel_GroupModelId",
                        column: x => x.ItemGroupKeyModel_GroupModelId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Items_Inventories_InventoryModelId",
                        column: x => x.InventoryModelId,
                        principalTable: "Inventories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Items_ItemCatalog_CatalogItemModelId",
                        column: x => x.CatalogItemModelId,
                        principalTable: "ItemCatalog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Items_Items_ItemModelWeaponId",
                        column: x => x.ItemModelWeaponId,
                        principalTable: "Items",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PhoneChats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ItemPhoneModelId = table.Column<int>(type: "integer", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsage = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhoneChats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhoneChats_Items_ItemPhoneModelId",
                        column: x => x.ItemPhoneModelId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PhoneContacts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ItemPhoneModelId = table.Column<int>(type: "integer", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsage = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhoneContacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhoneContacts_Items_ItemPhoneModelId",
                        column: x => x.ItemPhoneModelId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PhoneNotifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ItemPhoneModelId = table.Column<int>(type: "integer", nullable: false),
                    Context = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsage = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhoneNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhoneNotifications_Items_ItemPhoneModelId",
                        column: x => x.ItemPhoneModelId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PhoneMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChatModelId = table.Column<int>(type: "integer", nullable: false),
                    OwnerId = table.Column<int>(type: "integer", nullable: false),
                    Context = table.Column<string>(type: "text", nullable: false),
                    Local = table.Column<bool>(type: "boolean", nullable: false),
                    SenderPhoneNumber = table.Column<string>(type: "text", nullable: false),
                    TargetPhoneNumber = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsage = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhoneMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhoneMessages_PhoneChats_ChatModelId",
                        column: x => x.ChatModelId,
                        principalTable: "PhoneChats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BankAccountCharacterAccesses_CharacterModelId",
                table: "BankAccountCharacterAccesses",
                column: "CharacterModelId");

            migrationBuilder.CreateIndex(
                name: "IX_BankAccountGroupAccesses_GroupModelId",
                table: "BankAccountGroupAccesses",
                column: "GroupModelId");

            migrationBuilder.CreateIndex(
                name: "IX_BankHistoryEntryModel_BankAccountModelId",
                table: "BankHistoryEntryModel",
                column: "BankAccountModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Characters_AccountModelId",
                table: "Characters",
                column: "AccountModelId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatLogs_AccountModelId",
                table: "ChatLogs",
                column: "AccountModelId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatLogs_CharacterModelId",
                table: "ChatLogs",
                column: "CharacterModelId");

            migrationBuilder.CreateIndex(
                name: "IX_CommandLogs_AccountModelId",
                table: "CommandLogs",
                column: "AccountModelId");

            migrationBuilder.CreateIndex(
                name: "IX_CommandLogs_CharacterModelId",
                table: "CommandLogs",
                column: "CharacterModelId");

            migrationBuilder.CreateIndex(
                name: "IX_CriminalRecords_CharacterModelId",
                table: "CriminalRecords",
                column: "CharacterModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Deliveries_OrderGroupModelId",
                table: "Deliveries",
                column: "OrderGroupModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Deliveries_PlayerVehicleModelId",
                table: "Deliveries",
                column: "PlayerVehicleModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Deliveries_SupplierGroupModelId",
                table: "Deliveries",
                column: "SupplierGroupModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Directories_GroupModelId",
                table: "Directories",
                column: "GroupModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Doors_HouseModelId",
                table: "Doors",
                column: "HouseModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Files_DirectoryModelId",
                table: "Files",
                column: "DirectoryModelId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupMembers_CharacterModelId",
                table: "GroupMembers",
                column: "CharacterModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Houses_GroupModelId",
                table: "Houses",
                column: "GroupModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_CharacterModelId",
                table: "Inventories",
                column: "CharacterModelId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_HouseModelId",
                table: "Inventories",
                column: "HouseModelId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_ItemClothModelId",
                table: "Inventories",
                column: "ItemClothModelId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_VehicleModelId",
                table: "Inventories",
                column: "VehicleModelId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Items_CatalogItemModelId",
                table: "Items",
                column: "CatalogItemModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_InventoryModelId",
                table: "Items",
                column: "InventoryModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_ItemGroupKeyModel_GroupModelId",
                table: "Items",
                column: "ItemGroupKeyModel_GroupModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_ItemModelWeaponId",
                table: "Items",
                column: "ItemModelWeaponId");

            migrationBuilder.CreateIndex(
                name: "IX_MailAccountCharacterAccessModel_CharacterModelId",
                table: "MailAccountCharacterAccessModel",
                column: "CharacterModelId");

            migrationBuilder.CreateIndex(
                name: "IX_MailAccountGroupAccessModel_GroupModelId",
                table: "MailAccountGroupAccessModel",
                column: "GroupModelId");

            migrationBuilder.CreateIndex(
                name: "IX_MailLinkModel_MailModelId",
                table: "MailLinkModel",
                column: "MailModelId");

            migrationBuilder.CreateIndex(
                name: "IX_MdcAllergies_CharacterModelId",
                table: "MdcAllergies",
                column: "CharacterModelId");

            migrationBuilder.CreateIndex(
                name: "IX_MdcMedicalEntries_CharacterModelId",
                table: "MdcMedicalEntries",
                column: "CharacterModelId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderedVehicles_CatalogVehicleModelId",
                table: "OrderedVehicles",
                column: "CatalogVehicleModelId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalLicenses_CharacterModelId",
                table: "PersonalLicenses",
                column: "CharacterModelId");

            migrationBuilder.CreateIndex(
                name: "IX_PhoneChats_ItemPhoneModelId",
                table: "PhoneChats",
                column: "ItemPhoneModelId");

            migrationBuilder.CreateIndex(
                name: "IX_PhoneContacts_ItemPhoneModelId",
                table: "PhoneContacts",
                column: "ItemPhoneModelId");

            migrationBuilder.CreateIndex(
                name: "IX_PhoneMessages_ChatModelId",
                table: "PhoneMessages",
                column: "ChatModelId");

            migrationBuilder.CreateIndex(
                name: "IX_PhoneNotifications_ItemPhoneModelId",
                table: "PhoneNotifications",
                column: "ItemPhoneModelId");

            migrationBuilder.CreateIndex(
                name: "IX_PublicGarageEntries_CharacterModelId",
                table: "PublicGarageEntries",
                column: "CharacterModelId");

            migrationBuilder.CreateIndex(
                name: "IX_PublicGarageEntries_GroupModelId",
                table: "PublicGarageEntries",
                column: "GroupModelId");

            migrationBuilder.CreateIndex(
                name: "IX_PublicGarageEntries_PlayerVehicleModelId",
                table: "PublicGarageEntries",
                column: "PlayerVehicleModelId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleplayInfos_CharacterModelId",
                table: "RoleplayInfos",
                column: "CharacterModelId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRecordLogs_AccountModelId",
                table: "UserRecordLogs",
                column: "AccountModelId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRecordLogs_CharacterModelId",
                table: "UserRecordLogs",
                column: "CharacterModelId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRecordLogs_StaffAccountModelId",
                table: "UserRecordLogs",
                column: "StaffAccountModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_CharacterModelId",
                table: "Vehicles",
                column: "CharacterModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_GroupModelOwnerId",
                table: "Vehicles",
                column: "GroupModelOwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_Items_ItemClothModelId",
                table: "Inventories",
                column: "ItemClothModelId",
                principalTable: "Items",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_Characters_CharacterModelId",
                table: "Inventories");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_Characters_CharacterModelId",
                table: "Vehicles");

            migrationBuilder.DropForeignKey(
                name: "FK_Houses_Groups_GroupModelId",
                table: "Houses");

            migrationBuilder.DropForeignKey(
                name: "FK_Items_Groups_ItemGroupKeyModel_GroupModelId",
                table: "Items");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_Groups_GroupModelOwnerId",
                table: "Vehicles");

            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_Vehicles_VehicleModelId",
                table: "Inventories");

            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_Houses_HouseModelId",
                table: "Inventories");

            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_Items_ItemClothModelId",
                table: "Inventories");

            migrationBuilder.DropTable(
                name: "Animations");

            migrationBuilder.DropTable(
                name: "Appearances");

            migrationBuilder.DropTable(
                name: "BankAccountCharacterAccesses");

            migrationBuilder.DropTable(
                name: "BankAccountGroupAccesses");

            migrationBuilder.DropTable(
                name: "BankHistoryEntryModel");

            migrationBuilder.DropTable(
                name: "ChatLogs");

            migrationBuilder.DropTable(
                name: "CommandLogs");

            migrationBuilder.DropTable(
                name: "CriminalRecords");

            migrationBuilder.DropTable(
                name: "DefinedJobs");

            migrationBuilder.DropTable(
                name: "Deliveries");

            migrationBuilder.DropTable(
                name: "Doors");

            migrationBuilder.DropTable(
                name: "EmergencyCalls");

            migrationBuilder.DropTable(
                name: "FaceFeatures");

            migrationBuilder.DropTable(
                name: "Files");

            migrationBuilder.DropTable(
                name: "GroupMembers");

            migrationBuilder.DropTable(
                name: "GroupRanks");

            migrationBuilder.DropTable(
                name: "MailAccountCharacterAccessModel");

            migrationBuilder.DropTable(
                name: "MailAccountGroupAccessModel");

            migrationBuilder.DropTable(
                name: "MailLinkModel");

            migrationBuilder.DropTable(
                name: "MdcAllergies");

            migrationBuilder.DropTable(
                name: "MdcMedicalEntries");

            migrationBuilder.DropTable(
                name: "MdcNodes");

            migrationBuilder.DropTable(
                name: "OrderedVehicles");

            migrationBuilder.DropTable(
                name: "PersonalLicenses");

            migrationBuilder.DropTable(
                name: "PhoneContacts");

            migrationBuilder.DropTable(
                name: "PhoneMessages");

            migrationBuilder.DropTable(
                name: "PhoneNotifications");

            migrationBuilder.DropTable(
                name: "PublicGarageEntries");

            migrationBuilder.DropTable(
                name: "RegistrationOfficeEntries");

            migrationBuilder.DropTable(
                name: "RoleplayInfos");

            migrationBuilder.DropTable(
                name: "TattoosModel");

            migrationBuilder.DropTable(
                name: "UserRecordLogs");

            migrationBuilder.DropTable(
                name: "UserShopDatas");

            migrationBuilder.DropTable(
                name: "BankAccounts");

            migrationBuilder.DropTable(
                name: "Directories");

            migrationBuilder.DropTable(
                name: "MailAccounts");

            migrationBuilder.DropTable(
                name: "Mails");

            migrationBuilder.DropTable(
                name: "VehicleCatalog");

            migrationBuilder.DropTable(
                name: "PhoneChats");

            migrationBuilder.DropTable(
                name: "Characters");

            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "Groups");

            migrationBuilder.DropTable(
                name: "Vehicles");

            migrationBuilder.DropTable(
                name: "Houses");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "Inventories");

            migrationBuilder.DropTable(
                name: "ItemCatalog");
        }
    }
}
