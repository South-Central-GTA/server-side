using Microsoft.EntityFrameworkCore;
using Server.Database.Enums;
using Server.Database.Models;
using Server.Database.Models.Banking;
using Server.Database.Models.Character;
using Server.Database.Models.CustomLogs;
using Server.Database.Models.Delivery;
using Server.Database.Models.File;
using Server.Database.Models.Group;
using Server.Database.Models.Housing;
using Server.Database.Models.Inventory;
using Server.Database.Models.Inventory.Phone;
using Server.Database.Models.Mail;
using Server.Database.Models.Mdc;
using Server.Database.Models.Vehicles;

namespace Server.DataAccessLayer.Context;

public class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccountModel>().HasMany(a => a.UserRecords).WithOne(ur => ur.AccountModel)
            .HasForeignKey(ur => ur.AccountModelId);

        modelBuilder.Entity<MailAccountCharacterAccessModel>()
            .HasKey(m => new { m.MailAccountModelMailAddress, m.CharacterModelId });

        modelBuilder.Entity<MailAccountGroupAccessModel>()
            .HasKey(m => new { m.MailAccountModelMailAddress, GroupId = m.GroupModelId });

        modelBuilder.Entity<MailLinkModel>().HasKey(m => new { m.MailAccountModelMailAddress, m.MailModelId });

        modelBuilder.Entity<AppearancesModel>().HasKey(c => new { c.CharacterModelId });

        modelBuilder.Entity<FaceFeaturesModel>().HasKey(c => new { c.CharacterModelId });

        modelBuilder.Entity<DefinedJobModel>().HasKey(c => new { c.CharacterModelId });

        modelBuilder.Entity<BankAccountCharacterAccessModel>()
            .HasKey(c => new { c.BankAccountModelId, c.CharacterModelId });

        modelBuilder.Entity<BankAccountGroupRankAccessModel>()
            .HasKey(c => new { c.BankAccountModelId, c.GroupModelId });

        modelBuilder.Entity<GroupMemberModel>().HasKey(c => new { c.GroupModelId, c.CharacterModelId });

        modelBuilder.Entity<GroupRankModel>().HasKey(c => new { c.GroupModelId, c.Level });

        modelBuilder.Entity<LeaseCompanyHouseModel>().HasBaseType<HouseModel>();

        modelBuilder.Entity<HouseModel>().HasDiscriminator(h => h.HouseType).HasValue<HouseModel>(HouseType.HOUSE)
            .HasValue<LeaseCompanyHouseModel>(HouseType.COMPANY);

        modelBuilder.Entity<GroupModel>().HasDiscriminator(g => g.GroupType).HasValue<GroupModel>(GroupType.GROUP)
            .HasValue<CompanyGroupModel>(GroupType.COMPANY).HasValue<FactionGroupModel>(GroupType.FACTION)
            .HasValue<GangGroupModel>(GroupType.GANG).HasValue<MafiaGroupModel>(GroupType.MAFIA);

        modelBuilder.Entity<DeliveryModel>().HasDiscriminator(g => g.DeliveryType)
            .HasValue<DeliveryModel>(DeliveryType.DELIVERY).HasValue<ProductDeliveryModel>(DeliveryType.PRODUCT)
            .HasValue<VehicleDeliveryModel>(DeliveryType.VEHICLES);

        modelBuilder.Entity<ItemModel>().HasDiscriminator(i => i.ItemType).HasValue<ItemModel>(ItemType.NORMAL)
            .HasValue<ItemWeaponModel>(ItemType.WEAPON).HasValue<ItemWeaponAttachmentModel>(ItemType.WEAPON_ATTACHMENT)
            .HasValue<ItemWeaponAmmoModel>(ItemType.WEAPON_AMMO).HasValue<ItemClothModel>(ItemType.CLOTH)
            .HasValue<ItemFoodModel>(ItemType.FOOD).HasValue<ItemPhoneModel>(ItemType.PHONE)
            .HasValue<ItemKeyModel>(ItemType.KEY).HasValue<ItemGroupKeyModel>(ItemType.GROUP_KEY)
            .HasValue<ItemRadioModel>(ItemType.RADIO).HasValue<ItemHandCuffModel>(ItemType.HANDCUFF)
            .HasValue<ItemDrugModel>(ItemType.DRUG).HasValue<ItemPoliceTicketModel>(ItemType.POLICE_TICKET);

        modelBuilder.Entity<ItemClothModel>().HasOne(i => i.ClothingInventoryModel).WithOne(i => i.ItemClothModel)
            .HasForeignKey<InventoryModel>(i => i.ItemClothModelId);


        modelBuilder.Entity<RegistrationOfficeEntryModel>().HasKey(c => new { c.CharacterModelId });
    }

    #region Entities

    public DbSet<AccountModel> Accounts { get; set; }
    public DbSet<CharacterModel> Characters { get; set; }
    public DbSet<ChatLogModel> ChatLogs { get; set; }
    public DbSet<UserRecordLogModel> UserRecordLogs { get; set; }
    public DbSet<CommandLogModel> CommandLogs { get; set; }
    public DbSet<InventoryModel> Inventories { get; set; }
    public DbSet<ItemModel> Items { get; set; }
    public DbSet<ItemClothModel> ItemCloths { get; set; }
    public DbSet<ItemFoodModel> ItemFood { get; set; }
    public DbSet<ItemPhoneModel> ItemPhones { get; set; }
    public DbSet<ItemWeaponModel> ItemWeapons { get; set; }
    public DbSet<ItemWeaponAmmoModel> ItemWeaponAmmos { get; set; }
    public DbSet<ItemWeaponAttachmentModel> ItemWeaponAttachments { get; set; }
    public DbSet<ItemKeyModel> ItemKeys { get; set; }
    public DbSet<ItemRadioModel> ItemRadios { get; set; }
    public DbSet<ItemHandCuffModel> ItemHandCuffs { get; set; }
    public DbSet<ItemPoliceTicketModel> ItemPoliceTickets { get; set; }

    public DbSet<CatalogVehicleModel> VehicleCatalog { get; set; }
    public DbSet<PlayerVehicleModel> Vehicles { get; set; }
    public DbSet<AppearancesModel> Appearances { get; set; }
    public DbSet<FaceFeaturesModel> FaceFeatures { get; set; }
    public DbSet<PersonalLicenseModel> PersonalLicenses { get; set; }
    public DbSet<PhoneContactModel> PhoneContacts { get; set; }
    public DbSet<PhoneChatModel> PhoneChats { get; set; }
    public DbSet<PhoneMessageModel> PhoneMessages { get; set; }
    public DbSet<PhoneNotificationModel> PhoneNotifications { get; set; }
    public DbSet<HouseModel> Houses { get; set; }
    public DbSet<DoorModel> Doors { get; set; }
    public DbSet<BankAccountModel> BankAccounts { get; set; }
    public DbSet<BankAccountCharacterAccessModel> BankAccountCharacterAccesses { get; set; }
    public DbSet<BankAccountGroupRankAccessModel> BankAccountGroupAccesses { get; set; }
    public DbSet<PublicGarageEntryModel> PublicGarageEntries { get; set; }
    public DbSet<DefinedJobModel> DefinedJobs { get; set; }
    public DbSet<GroupModel> Groups { get; set; }
    public DbSet<CompanyGroupModel> CompanyGroups { get; set; }
    public DbSet<FactionGroupModel> FactionGroups { get; set; }
    public DbSet<GangGroupModel> GangGroups { get; set; }
    public DbSet<MafiaGroupModel> MafiaGroups { get; set; }
    public DbSet<GroupMemberModel> GroupMembers { get; set; }
    public DbSet<GroupRankModel> GroupRanks { get; set; }
    public DbSet<DeliveryModel> Deliveries { get; set; }
    public DbSet<CatalogItemModel> ItemCatalog { get; set; }
    public DbSet<OrderedVehicleModel> OrderedVehicles { get; set; }
    public DbSet<MailAccountModel> MailAccounts { get; set; }
    public DbSet<MailModel> Mails { get; set; }
    public DbSet<UserShopDataModel> UserShopDatas { get; set; }
    public DbSet<AnimationModel> Animations { get; set; }
    public DbSet<RoleplayInfoModel> RoleplayInfos { get; set; }
    public DbSet<EmergencyCallModel> EmergencyCalls { get; set; }
    public DbSet<CriminalRecordModel> CriminalRecords { get; set; }
    public DbSet<MdcNoteModel> MdcNodes { get; set; }
    public DbSet<MdcMedicalEntryModel> MdcMedicalEntries { get; set; }
    public DbSet<MdcAllergyModel> MdcAllergies { get; set; }
    public DbSet<FileModel> Files { get; set; }
    public DbSet<DirectoryModel> Directories { get; set; }
    public DbSet<RegistrationOfficeEntryModel> RegistrationOfficeEntries { get; set; }
    public DbSet<BulletInEntryModel> BulletInEntries { get; set; }

    #endregion
}