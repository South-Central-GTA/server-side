using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using Server.Core.Abstractions;
using Server.Core.Callbacks;
using Server.Core.Configuration;
using Server.Core.Entities;
using Server.DataAccessLayer.Context;
using Server.Database.Enums;
using Server.Database.Models;
using Server.Database.Models.Character;
using Server.Database.Models.Group;

namespace Server.ServerJobs;

public class Database : IServerJob
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;
    private readonly DevelopmentOptions _devOptions;
    private readonly ILogger<Database> _logger;

    public Database(
        ILogger<Database> logger,
        IDbContextFactory<DatabaseContext> dbContextFactory,
        IOptions<DevelopmentOptions> devOptions)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _devOptions = devOptions.Value;
    }

    public Task OnSave()
    {
        var playersTask = Task.Run(async () =>
        {
            var charsToUpdate = new List<CharacterModel>();
            var callback = new AsyncFunctionCallback<IPlayer>(async player =>
            {
                var serverPlayer = (ServerPlayer)player;

                if (serverPlayer.IsSpawned)
                {
                    serverPlayer.CharacterModel.Position = serverPlayer.Position;
                    serverPlayer.CharacterModel.Rotation = serverPlayer.Rotation;

                    serverPlayer.CharacterModel.Health = serverPlayer.Health;
                    serverPlayer.CharacterModel.Armor = serverPlayer.Armor;

                    charsToUpdate.Add(serverPlayer.CharacterModel);
                }

                await Task.CompletedTask;
            });

            await Alt.ForEachPlayers(callback);

            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            dbContext.Characters.UpdateRange(charsToUpdate);
            await dbContext.SaveChangesAsync();
        });

        return Task.WhenAll(playersTask);
    }

    public async Task OnShutdown()
    {
        await Task.CompletedTask;
    }

    public async Task OnStartup()
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        if (_devOptions.DropDatabaseAtStartup)
        {
            if (!_devOptions.LocalDb)
            {
                await dbContext.Database.ExecuteSqlRawAsync("GRANT CONNECT ON DATABASE scdb TO public;");
            }

            await dbContext.Database.EnsureDeletedAsync();
            _logger.LogWarning("Database dropped.");
        }

        await dbContext.Database.MigrateAsync();

        if (_devOptions.SeedingDefaultDataIntoDatabase)
        {
            await dbContext.ItemCatalog.AddRangeAsync(new List<CatalogItemModel>
            {
                new(ItemCatalogIds.DOLLAR, "Dollar", "prop_money_bag_01", Rotation.Zero, 0, "money", "Geld regiert die Welt. Kaufe Dir dein Glück in Los Santos oder halte Dich einfach nur über Wasser.", Rarity.POOR, 0.0001f, false, true, false, false, 0, 0),
                new(ItemCatalogIds.PHONE, "Handy", "prop_amb_phone", new Rotation(270, 0, 0), 0.02f, "phone", "Bleibe ständig mit Deinen Bekanntschaften in Kontakt oder schließe den Deal deines Lebens ab.", Rarity.COMMON, 0.1f, false, false, true, false, 850, 600, 5),
                new(ItemCatalogIds.KEY, "Schlüssel", "p_car_keys_01", Rotation.Zero, 0.03f, "key", "Zu jedem Schloss gibt es auch den passenden Schlüssel. Doch zu welchem Schloss passt dieser Schlüssel?", Rarity.COMMON, 0.1f, false, false, false, true, 0, 10),
                new(ItemCatalogIds.CLOTHING_HAT, "ClothingHat", "prop_cs_box_clothes", Rotation.Zero, 0.04f, "hat", "Ein Kleidungsstück um den Kopf zu bedecken. Einige nutzen ihn zum Schutz vor Sonne, andere vor Kälte.", Rarity.POOR, 0.05f, true, false, false, true, 100, 50),
                new(ItemCatalogIds.CLOTHING_GLASSES, "ClothingGlasses", "prop_cs_box_clothes", Rotation.Zero, 0.04f, "glasses", "Einige behaupten, mit einer Brille sähe man schlauer aus. Doch eigentlich kann man damit einfach nur besser sehen.", Rarity.POOR, 0.1f, true, false, false, true, 100, 50),
                new(ItemCatalogIds.CLOTHING_EARS, "ClothingEars", "prop_cs_box_clothes", Rotation.Zero, 0.04f, "ears", "Das Ohr ist ein Sinnesorgang mit dem verschiedene Töne wahrgenommen werden. Einige schmücken es aber auch. ", Rarity.POOR, 0.1f, true, false, false, true, 500, 250),
                new(ItemCatalogIds.CLOTHING_MASK, "ClothingMask", "prop_cs_box_clothes", Rotation.Zero, 0.04f, "mask", "Manchmal schadet es nicht sein Gesicht zu verdecken, wenn man einen schlechten Tag hat..", Rarity.POOR, 0.1f, true, false, false, true, 50, 25),
                new(ItemCatalogIds.CLOTHING_TOP, "ClothingTop", "prop_cs_box_clothes", Rotation.Zero, 0.04f, "top", "Oberteile gibt es in verschiedenen Variationen. Über die Jahre wurde eine ziemlich große Auswahl an unterschiedlichen Oberteilen erschaffen.", Rarity.POOR, 0.1f, true, false, false, true, 50, 25),
                new(ItemCatalogIds.CLOTHING_UNDERSHIRT, "ClothingUndershirt", "prop_cs_box_clothes", Rotation.Zero, 0.04f, "undershirt", "Unterhemden werden in der Regel unter Oberteilen getragen. Sie können aber einfach so getragen werden.", Rarity.POOR, 0.1f, true, false, false, true, 25, 12),
                new(ItemCatalogIds.CLOTHING_ACCESSORIES, "ClothingAccessories", "prop_cs_box_clothes", Rotation.Zero, 0.04f, "accessories", "Das i-Tüpfelchen welches dein optisches Aussehen abrundet.", Rarity.POOR, 0.1f, true, false, false, false, 250, 125),
                new(ItemCatalogIds.CLOTHING_WATCH, "ClothingWatch", "prop_cs_box_clothes", Rotation.Zero, 0.04f, "watch", "Ein Gegenstand mit dem man die Uhrzeit ablesen kann. Manche nutzen sie aber auch einfach als Statussymbol.", Rarity.POOR, 0.1f, true, false, false, true, 500, 250),
                new(ItemCatalogIds.CLOTHING_BRACELET, "ClothingBracelet", "prop_cs_box_clothes", Rotation.Zero, 0.04f, "bracelet", "Manche Personen tragen es um das Handgelenk. Manche sind schön, andere weniger.", Rarity.POOR, 0.1f, true, false, false, true, 500, 250),
                new(ItemCatalogIds.CLOTHING_PANTS, "ClothingPants", "prop_cs_box_clothes", Rotation.Zero, 0.04f, "pants", "Die meiste Zeit solltest du eine anhaben…", Rarity.POOR, 0.1f, true, false, false, true, 50, 25),
                new(ItemCatalogIds.CLOTHING_BACKPACK, "ClothingBackpack", "prop_cs_box_clothes", Rotation.Zero, 0.04f, "backpack", "Der Rucksack ist ein Behälter aus Stoff, flexiblem Kunststoff oder Leder, der an Gurten auf dem Rücken getragen wird.", Rarity.POOR, 6f, true, false, false, true, 100, 50),
                new(ItemCatalogIds.CLOTHING_BODY_ARMOR, "ClothingBodyArmor", "prop_cs_box_clothes", Rotation.Zero, 0.04f, "body_armor", "Eine kugelsichere Weste hat schon so einige Leben gerettet.", Rarity.POOR, 2f, true, false, false, true, 150, 75, 1),
                new(ItemCatalogIds.CLOTHING_SHOES, "ClothingShoes", "prop_cs_box_clothes", Rotation.Zero, 0.04f, "shoes", "Schuhe sind wichtig. Oder willst du offene Füße haben?", Rarity.POOR, 0.2f, true, false, false, true, 50, 25),
                new(ItemCatalogIds.NON_ALC_DRINKS, "Softdrinks", "prop_ld_flow_bottle", Rotation.Zero, 0.11f, "soft_drink", "Es gibt sie in zahlreichen Geschmäckern. Hier sollte jeder Gaumen fündig werden. Jedoch ziemlich ungesund.", Rarity.POOR, 0.1f, false, false, true, true, 5, 2),
                new(ItemCatalogIds.ALC_DRINKS, "Alkoholisches Getränk ", "ba_prop_battle_whiskey_bottle_s", Rotation.Zero, 0.15f, "liquor", "Perfekt um sich die Sinne zu benebeln. Vorsicht: Hohes Suchtpotenzial!", Rarity.POOR, 0.1f, false, false, true, true, 10, 5),
                new(ItemCatalogIds.FAST_FOOD, "Fastfood", "prop_food_bag1", Rotation.Zero, 0, "fast_food", "Perfekt für den Hunger zwischendurch geeignet. ", Rarity.POOR, 0.1f, false, false, true, true, 5, 2),
                new(ItemCatalogIds.HEALTHY_FOOD, "Obst & Gemüse", "hei_prop_hei_paper_bag", Rotation.Zero, 0.15f, "healthy_food", "Obst bzw. ist für den Menschen roh genießbar. Es wird von Bäumen oder Sträuchern geerntet.", Rarity.POOR, 0.1f, false, true, true, true, 3, 1),
                new(ItemCatalogIds.BREAD, "Brot", "v_ret_247_bread1", Rotation.Zero, 0.1f, "bread", "Eines der ältesten Lebensmittel der Menschheit. Es wird aus Mehl und Wasser gebacken.", Rarity.POOR, 0.1f, false, false, true, true, 8, 4),
                new(ItemCatalogIds.SANDWICH, "Sandwich", "prop_sandwich_01", Rotation.Zero, 0.03f, "sandwich", "Es besteht aus zwei dünnen Brotscheiben. Dazwischen befindet sich je nach Geschmack unteschiedlichster Belag.", Rarity.POOR, 0.1f, false, false, true, true, 10, 5),
                new(ItemCatalogIds.SOUP, "Suppe", "prop_paints_can04", Rotation.Zero, 0.1f, "soup", "Eine Suppe ist eine Speise mit hohem Flüssigkeitsanteil. Meistens wird sie als Vorspeise serviert.", Rarity.POOR, 0.1f, false, false, true, true, 15, 5),
                new(ItemCatalogIds.MEAT, "Tierfleisch", "ng_proc_box_01a", Rotation.Zero, 0, "meat", "Tierfleisch ist das Produkt aus verarbeiteten Tieren. Es ist das am meist verzehrteste Lebensmittel der Gesellschaft.", Rarity.POOR, 0.1f, false, false, true, true, 20, 15),
                new(ItemCatalogIds.SWEETS, "Backware", "ng_proc_box_01a", Rotation.Zero, 0, "sweets", "Backwaren sind Lebensmittel die aus Getreide und Wasser erzeugt werden. In der Regel stellt sie ein Bäcker oder Konditor her.", Rarity.POOR, 0.1f, false, true, true, true, 6, 3),
                new(ItemCatalogIds.CANDY, "Süßigkeit", "prop_candy_pqs", Rotation.Zero, 0.03f, "candy", "Süßwaren sind Lebensmittel die den Zuckerspiegel in die Höhe schießen lassen. Übermäßiger Verzehr führt zu Übergwicht und schlechten Zähnen.", Rarity.POOR, 0.1f, false, true, true, true, 2, 1),
                new(ItemCatalogIds.GROUP_KEY, "Gruppenschlüssel", "p_car_keys_01", Rotation.Zero, 0.03f, "group-key", "Gruppenschlüssel scheinen eine Art biometrische Sicherung zu haben, nur Mitglieder von einer bestimmten Gruppe können die Schlüssel benutzen.", Rarity.COMMON, 0.1f, false, false, false, true, 500, 100),
                new(ItemCatalogIds.HANDCUFF_KEY, "Handschellenschlüssel", "prop_cuff_keys_01", Rotation.Zero, 0.03f, "handcuffs-key", "Ein Schlüssel, welcher zu einer Handschelle gehört, passt genau einmal.", Rarity.COMMON, 0.1f, false, false, false, true, 50, 25),
                new(ItemCatalogIds.RADIO, "Funkgerät", "prop_cs_hand_radio", new Rotation(270, 0, 0), 0.03f, "radio", "Ein Funkgerät ist ein elektrisches Gerät, das mithilfe der Funktechnik der drahtlosen Kommunikation dient.", Rarity.COMMON, 0.2f, false, false, false, true, 1500, 750),
                new(ItemCatalogIds.MEGAPHONE, "Megafon", "prop_megaphone_01", new Rotation(90, 0, 0), 0.05f, "megaphone", "Ein Megafon lenkt die Ausbreitung von Schall und damit die Verständlichkeit von gesprochener Sprache.", Rarity.COMMON, 0.9f, false, false, false, true, 500, 250),
                new(ItemCatalogIds.TRAFFIC_CONE, "Verkehrskegel", "prop_mp_cone_02", Rotation.Zero, 0, "cone", "Perfekt um eine Straße zu sperren und den Verkehr zu leiten.", Rarity.COMMON, 0.1f, false, false, false, false, 10, 5),
                new(ItemCatalogIds.HANDCUFF, "Handschelle", "prop_cs_cuffs_01", new Rotation(30, 0, 0), -0.03f, "handcuffs", "Umgangssprachlich auch Acht oder Achter genannt, dient zur Fesselung von Personen.", Rarity.COMMON, 0.2f, false, true, false, true, 100, 50),
                new(ItemCatalogIds.LICENSES, "Lizenzen", "prop_cd_paper_pile1", new Rotation(90, 0, 0), 0.011f, "license", "Eine Auflistung an Lizenzen welche die Person hat.", Rarity.COMMON, 0.1f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.REPAIR_KIT, "Werkzeugkasten", "prop_tool_box_01", Rotation.Zero, 0, "repairkit", "Ein Werkzeugkasten, womit man als ausgebildeter Mechaniker im richtigen Umfeld ein Fahrzeug reparieren kann.", Rarity.COMMON, 3.0f, false, false, true, false, 300, 0),
                new(ItemCatalogIds.WEAPON_ANTIQUE_CAVALRY_DAGGER, "Antiker Kavallerie-Dolch", "prop_w_me_dagger", new Rotation(90, 0, 0), 0.01f, "knife", "Waffe", Rarity.EPIC, 0.3f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_BASEBALL_BAT, "Baseballschläger", "p_cs_bbbat_01", new Rotation(90, 0, 0), 0.01f, "baseballbat", "Waffe", Rarity.COMMON, 0.5f, false, false, true, false, 100, 0),
                new(ItemCatalogIds.WEAPON_BROKEN_BOTTLE, "Zerbrochene Flasche", "prop_w_me_bottle", new Rotation(90, 0, 0), 0, "broken-bottle", "Waffe", Rarity.POOR, 0.1f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_CROWBAR, "Brechstange", "prop_ing_crowbar", new Rotation(90, 0, 0), 0, "crowbar", "Waffe", Rarity.POOR, 0.5f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_FLASHLIGHT, "Taschenlampe", "prop_scn_police_torch", new Rotation(90, 0, 0), 0, "flashlight", "Waffe", Rarity.POOR, 0.2f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_GOLF_CLUB, "Golfschläger", "prop_golf_pitcher_01", new Rotation(90, 0, 0), 0, "golfclub", "Waffe", Rarity.COMMON, 0.4f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_HAMMER, "Hammer", "prop_tool_hammer", new Rotation(90, 0, 0), 0, "hammer", "Waffe", Rarity.COMMON, 0.4f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_HATCHET, "Beil", "prop_w_me_hatchet", new Rotation(90, 0, 0), 0.01f, "weapon_hatchet", "Waffe", Rarity.COMMON, 0.4f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_BRASS_KNUCKLES, "Schlagring", "prop_box_guncase_01a", Rotation.Zero, 0, "knuckles", "Waffe", Rarity.COMMON, 0.5f, false, false, true, false, 30, 0),
                new(ItemCatalogIds.WEAPON_KNIFE, "Messer", "prop_w_me_knife_01", new Rotation(90, 0, 0), 0, "knife", "Waffe", Rarity.POOR, 0.4f, false, false, true, false, 220, 0),
                new(ItemCatalogIds.WEAPON_MACHETE, "Machete", "prop_ld_w_me_machette", new Rotation(90, 0, 0), 0, "machete", "Waffe", Rarity.POOR, 0.6f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_NIGHTSTICK, "Schlagstock", "prop_box_guncase_01a", Rotation.Zero, 0, "nightstick", "Waffe", Rarity.POOR, 0.6f, false, false, true, false, 185, 0),
                new(ItemCatalogIds.WEAPON_PIPE_WRENCH, "Rohrzange", "prop_tool_wrench", new Rotation(90, 0, 0), 0.02f, "pipe-wrench", "Waffe", Rarity.POOR, 0.9f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_POOL_CUE, "Billardqueue", "prop_pool_cue", new Rotation(90, 0, 0), 0, "cue", "Waffe", Rarity.POOR, 0.3f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_STONE_HATCHET, "Steinbeil", "prop_w_me_hatchet", new Rotation(90, 0, 0), 0, "weapon_hatchet", "Waffe", Rarity.EPIC, 0.3f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_SWITCHBLADE, "Springmesser", "prop_w_me_knife_01", new Rotation(90, 0, 0), 0, "knife", "Waffe", Rarity.RARE, 0.3f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_AP_PISTOL, "AP Pistole", "prop_box_guncase_01a", Rotation.Zero, 0, "weapon_pistol", "Waffe", Rarity.COMMON, 0.3f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_COMBAT_PISTOL, "Gefechtspistole", "prop_box_guncase_01a", Rotation.Zero, 0, "weapon_pistol", "Waffe", Rarity.COMMON, 0.3f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_DOUBLE_ACTION_REVOLVER, "Double Action Revolver", "prop_box_guncase_01a", Rotation.Zero, 0, "weapon_pistol", "Waffe", Rarity.EPIC, 0.5f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_FLARE_GUN, "Leuchtpistole", "prop_box_guncase_01a", Rotation.Zero, 0, "flaregun", "Waffe", Rarity.COMMON, 0.4f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_HEAVY_PISTOL, "Schwere Pistole", "prop_box_guncase_01a", Rotation.Zero, 0, "weapon_pistol", "Waffe", Rarity.COMMON, 0.5f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_HEAVY_REVOLVER, "Schwere Revolver", "prop_box_guncase_01a", Rotation.Zero, 0, "weapon_pistol", "Waffe", Rarity.COMMON, 0.5f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_HEAVY_REVOLVER_MK_II, "Schwere Revolver MkII", "prop_box_guncase_01a", Rotation.Zero, 0, "weapon_pistol", "Waffe", Rarity.COMMON, 0.5f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_MARKSMAN_PISTOL, "Scharfschützenpistole", "prop_box_guncase_01a", Rotation.Zero, 0, "weapon_pistol", "Waffe", Rarity.EPIC, 0.5f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_PISTOL, "Pistole", "prop_box_guncase_01a", Rotation.Zero, 0, "weapon_pistol", "Waffe", Rarity.COMMON, 0.5f, false, false, true, false, 2100, 0),
                new(ItemCatalogIds.WEAPON_PISTOL50, "Pistole .50", "prop_box_guncase_01a", Rotation.Zero, 0, "weapon_pistol", "Waffe", Rarity.COMMON, 0.5f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_PISTOL_MK_II, "Pistole Mk II", "prop_box_guncase_01a", Rotation.Zero, 0, "weapon_pistol", "Waffe", Rarity.COMMON, 0.5f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_SNS_PISTOL, "Billigknarre", "prop_box_guncase_01a", Rotation.Zero, 0, "weapon_pistol", "Waffe", Rarity.POOR, 0.5f, false, false, true, false, 430, 0),
                new(ItemCatalogIds.WEAPON_SNS_PISTOL_MK_II, "Billigknarre Mk II", "prop_box_guncase_01a", Rotation.Zero, 0, "weapon_pistol", "Waffe", Rarity.POOR, 0.5f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_STUN_GUN, "Elektroschocker", "prop_box_guncase_01a", Rotation.Zero, 0, "stungun", "Waffe", Rarity.POOR, 0.5f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_VINTAGE_PISTOL, "Vintage Pistole", "prop_box_guncase_01a", Rotation.Zero, 0, "weapon_pistol", "Waffe", Rarity.POOR, 0.5f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_ASSAULT_SMG, "Sturm-SMG", "prop_idol_case_02", Rotation.Zero, 0, "weapon_submachinegun", "Waffe", Rarity.COMMON, 0.5f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_COMBAT_PDW, "Gefechts-PDW", "prop_idol_case_02", Rotation.Zero, 0, "weapon_submachinegun", "Waffe", Rarity.COMMON, 0.5f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_MACHINE_PISTOL, "Maschinenpistole", "prop_box_guncase_01a", Rotation.Zero, 0, "weapon_submachinegun", "Waffe", Rarity.COMMON, 0.5f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_MICRO_SMG, "Mikro-SMG", "prop_idol_case_02", Rotation.Zero, 0, "weapon_submachinegun", "Waffe", Rarity.COMMON, 0.5f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_MINI_SMG, "Mini-SMG", "prop_idol_case_02", Rotation.Zero, 0, "weapon_submachinegun", "Waffe", Rarity.COMMON, 0.5f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_SMG, "SMG", "prop_idol_case_02", Rotation.Zero, 0, "weapon_submachinegun", "Waffe", Rarity.COMMON, 0.5f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_SMG_MK_II, "SMG Mk II", "prop_idol_case_02", Rotation.Zero, 0, "weapon_submachinegun", "Waffe", Rarity.COMMON, 0.5f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_ASSAULT_SHOTGUN, "Sturmschrotflinte", "ch_prop_ch_guncase_01a", new Rotation(90, 0, 0), 0.018f, "weapon_shotgun", "Waffe", Rarity.COMMON, 0.5f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_BULLPUP_SHOTGUN, "Bullpup-Schrotflinte", "ch_prop_ch_guncase_01a", new Rotation(90, 0, 0), 0.018f, "weapon_shotgun", "Waffe", Rarity.UNCOMMON, 0.5f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_COMBAT_SHOTGUN, "Combat-Schrotflinte", "ch_prop_ch_guncase_01a", new Rotation(90, 0, 0), 0.018f, "weapon_shotgun", "Waffe", Rarity.RARE, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_DOUBLE_BARREL_SHOTGUN, "Doppelläufige Schrotflinte", "ch_prop_ch_guncase_01a", new Rotation(90, 0, 0), 0.018f, "weapon_shotgun", "Waffe", Rarity.UNCOMMON, 0.5f, false, false, true, false, 3000, 0),
                new(ItemCatalogIds.WEAPON_HEAVY_SHOTGUN, "Schwere Schrotflinte", "ch_prop_ch_guncase_01a", new Rotation(90, 0, 0), 0.018f, "weapon_shotgun", "Waffe", Rarity.UNCOMMON, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_MUSKET, "Muskete", "ch_prop_ch_guncase_01a", new Rotation(90, 0, 0), 0.018f, "musket", "Waffe", Rarity.EPIC, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_PUMP_SHOTGUN, "Pump Shotgun", "ch_prop_ch_guncase_01a", new Rotation(90, 0, 0), 0.018f, "weapon_shotgun", "Waffe", Rarity.COMMON, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_PUMP_SHOTGUN_MK_II, "Pump Schrotflinte Mk II", "ch_prop_ch_guncase_01a", new Rotation(90, 0, 0), 0.018f, "weapon_shotgun", "Waffe", Rarity.UNCOMMON, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_SAWED_OFF_SHOTGUN, "Abgesägte Schrotflinte", "ch_prop_ch_guncase_01a", new Rotation(90, 0, 0), 0.018f, "weapon_shotgun", "Waffe", Rarity.UNCOMMON, 0.6f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_SWEEPER_SHOTGUN, "Trommel Schrotflinte", "ch_prop_ch_guncase_01a", new Rotation(90, 0, 0), 0.018f, "weapon_shotgun", "Waffe", Rarity.UNCOMMON, 0.6f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_ADVANCED_RIFLE, "Kampfgewehr", "ch_prop_ch_guncase_01a", new Rotation(90, 0, 0), 0.018f, "weapon_rifle", "Waffe", Rarity.UNCOMMON, 0.6f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_ASSAULT_RIFLE, "Sturmgewehr", "ch_prop_ch_guncase_01a", new Rotation(90, 0, 0), 0.018f, "weapon_assault", "Waffe", Rarity.UNCOMMON, 0.6f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_ASSAULT_RIFLE_MK_II, "Sturmgewehr Mk II", "ch_prop_ch_guncase_01a", new Rotation(90, 0, 0), 0.018f, "weapon_assault", "Waffe", Rarity.UNCOMMON, 1.1f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_BULLPUP_RIFLE, "Bullpup-Gewehr", "ch_prop_ch_guncase_01a", new Rotation(90, 0, 0), 0.018f, "weapon_rifle", "Waffe", Rarity.UNCOMMON, 1.1f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_BULLPUP_RIFLE_MK_II, "Bullpup-Gewehr Mk II", "ch_prop_ch_guncase_01a", new Rotation(90, 0, 0), 0.018f, "weapon_rifle", "Waffe", Rarity.UNCOMMON, 1.1f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_CARBINE_RIFLE, "Karabiner", "ch_prop_ch_guncase_01a", new Rotation(90, 0, 0), 0.018f, "weapon_rifle", "Waffe", Rarity.UNCOMMON, 1.1f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_CARBINE_RIFLE_MK_II, "Karabiner Mk II", "prop_box_guncase_01a", Rotation.Zero, 0, "weapon_rifle", "Waffe", Rarity.RARE, 1.1f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_COMPACT_RIFLE, "Kompaktes Gewehr", "prop_box_guncase_01a", Rotation.Zero, 0, "weapon_assault", "Waffe", Rarity.COMMON, 0.6f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_MILITARY_RIFLE, "Militärgewehr", "prop_box_guncase_01a", Rotation.Zero, 0, "weapon_rifle", "Waffe", Rarity.COMMON, 0.6f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_SPECIAL_CARBINE, "Spezial-Karabiner", "ch_prop_ch_guncase_01a", new Rotation(90, 0, 0), 0.018f, "weapon_rifle", "Waffe", Rarity.COMMON, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_SPECIAL_CARBINE_MK_II, "Spezial-Karabiner Mk II", "ch_prop_ch_guncase_01a", new Rotation(90, 0, 0), 0.018f, "weapon_rifle", "Waffe", Rarity.RARE, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_COMBAT_MG, "Gefechts-MG", "ch_prop_ch_guncase_01a", new Rotation(90, 0, 0), 0.018f, "weapon_lightmachinegun", "Waffe", Rarity.RARE, 10.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_COMBAT_MG_MK_II, "Gefechts-MG Mk II", "ch_prop_ch_guncase_01a", new Rotation(90, 0, 0), 0.018f, "weapon_lightmachinegun", "Waffe", Rarity.EPIC, 12.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_GUSENBERG_SWEEPER, "Gusenberg-Bleispritze", "ch_prop_ch_guncase_01a", new Rotation(90, 0, 0), 0.018f, "weapon_assault", "Waffe", Rarity.EPIC, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_MG, "MG", "ch_prop_ch_guncase_01a", new Rotation(90, 0, 0), 0.018f, "weapon_lightmachinegun", "Waffe", Rarity.EPIC, 11.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_HEAVY_SNIPER, "Schweres Scharfschützengewehr", "ch_prop_ch_guncase_01a", new Rotation(90, 0, 0), 0.018f, "weapon_sniper", "Waffe", Rarity.LEGENDARY, 11.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_HEAVY_SNIPER_MK_II, "Schweres Scharfschützengewehr Mk II", "ch_prop_ch_guncase_01a", new Rotation(90, 0, 0), 0.018f, "weapon_sniper", "Waffe", Rarity.LEGENDARY, 11.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_MARKSMAN_RIFLE, "Präzisionsgewehr", "ch_prop_ch_guncase_01a", new Rotation(90, 0, 0), 0.018f, "weapon_sniper", "Waffe", Rarity.LEGENDARY, 6.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_MARKSMAN_RIFLE_MK_II, "Präzisionsgewehr Mk II", "ch_prop_ch_guncase_01a", new Rotation(90, 0, 0), 0.018f, "weapon_sniper", "Waffe", Rarity.LEGENDARY, 8.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_SNIPER_RIFLE, "Scharfschützengewehr", "ch_prop_ch_guncase_01a", new Rotation(90, 0, 0), 0.018f, "weapon_sniper", "Waffe", Rarity.LEGENDARY, 9.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_BASEBALL, "Baseball", "prop_tennis_ball", Rotation.Zero, 0.06f, "ball", "Ein Ball ist ein rundes, üblicherweise kugelförmiges, seltener ovales, elastisches Spielzeug oder Sportgerät aus Leder, Gummi oder Kunststoff.", Rarity.POOR, 0.2f, false, true, false, false, 0, 0, 1),
                new(ItemCatalogIds.WEAPON_BZ_GAS, "BZ Gasgranate", "prop_gas_grenade", new Rotation(90, 0, 0), 0.05f, "bzgas", "Waffe", Rarity.POOR, 0.2f, false, true, false, false, 0, 0, 10),
                new(ItemCatalogIds.WEAPON_FLARE, "Flare", "prop_flare_01b", new Rotation(90, 0, 0), 0.03f, "flare", "Ein heller Leuchtkörper welcher auch bei schlechten Wetter ein helles Licht abgibt.", Rarity.COMMON, 0.2f, false, true, false, false, 0, 0, 10),
                new(ItemCatalogIds.WEAPON_GRENADE, "Granate", "prop_box_guncase_01a", Rotation.Zero, 0, "grenade", "Waffe", Rarity.COMMON, 0.2f, false, true, false, false, 0, 0, 10),
                new(ItemCatalogIds.WEAPON_MOLOTOV_COCKTAIL, "Molotow-Cocktail", "prop_box_guncase_01a", Rotation.Zero, 0, "molotov-cocktail", "Waffe", Rarity.COMMON, 0.5f, false, true, false, false, 0, 0, 5),
                new(ItemCatalogIds.WEAPON_SNOWBALL, "Schneball", "ch_prop_ch_guncase_01a", new Rotation(90, 0, 0), 0.018f, "snowball", "Der gewöhnliche Schneeball.", Rarity.POOR, 0.1f, false, true, false, false, 0, 0, 10),
                new(ItemCatalogIds.WEAPON_FIRE_EXTINGUISHER, "Feuerlöscher", "prop_fire_exting_1a", Rotation.Zero, -0.01f, "fire-extinguisher", "Ein Feuerlöscher ist ein tragbares Kleinlöschgerät mit einer Gesamtmasse von maximal 20 Kilogramm. Er dient dem Ablöschen von Kleinbränden.", Rarity.POOR, 1.5f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_JERRY_CAN, "Kanister", "prop_jerrycan_01a", Rotation.Zero, -0.01f, "jerrycan", "Kanister mit Benzin drinnen.", Rarity.COMMON, 3.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.WEAPON_PARACHUTE, "Fallschirm", "p_parachute_s", new Rotation(90, 0, 0), 0.04f, "parachute", "Ein Fallschirm ist ein technisches Gerät, das dazu dient, eine Person oder einen Gegenstand aus großer Höhe unversehrt auf den Boden zu bringen.", Rarity.COMMON, 0.5f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.AMMO_PISTOL, "Pistolen Munition", "prop_box_ammo07a", Rotation.Zero, 0, "ammo_pistol", "Kann in eine Handfeuerwaffe geladen werden um abgefeuert zu werden.", Rarity.COMMON, 0.001f, false, true, true, false, 10, 0),
                new(ItemCatalogIds.AMMO_MACHINE_GUN, "SMG Munition", "prop_ld_ammo_pack_03", Rotation.Zero, 0.01f, "ammo_submachinegun", "Kann in ein SMG geladen werden um abgefeuert zu werden.", Rarity.COMMON, 0.001f, false, true, false, false, 0, 0),
                new(ItemCatalogIds.AMMO_ASSAULT, "Gewehr Munition", "prop_ld_ammo_pack_01", Rotation.Zero, 0.01f, "ammo_assault", "Kann in ein Gewehr geladen werden um abgefeuert zu werden.", Rarity.COMMON, 0.001f, false, true, false, false, 0, 0),
                new(ItemCatalogIds.AMMO_SNIPER, "Scharfschützengewehr Munition", "prop_box_ammo07b", Rotation.Zero, -0.01f, "ammo_sniper", "Kann in ein Scharfschützengewehr geladen werden um abgefeuert zu werden.", Rarity.COMMON, 0.1f, false, true, false, true, 0, 0),
                new(ItemCatalogIds.AMMO_SHOTGUN, "Schrotflinten Pellets", "prop_ld_ammo_pack_02", Rotation.Zero, 0.01f, "ammo_shotgun", "Kann in eine Schrotflinte werden um abgefeuert zu werden.", Rarity.COMMON, 0.001f, false, true, true, true, 20, 0),
                new(ItemCatalogIds.AMMO_LIGHT_MACHINE_GUN, "Schwere Munition", "prop_box_ammo01a", Rotation.Zero, 0, "ammo_lightmachinegun", "Kann in ein Maschinengewehr geladen werden um abgefeuert zu werden.", Rarity.EPIC, 0.2f, false, true, false, true, 0, 0),
                new(ItemCatalogIds.COMPONENT_KNUCKLE_VARMOD_PIMP, "Schlagring 'Der Pimp' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung auf einen Schlagring gezogen werden.", Rarity.EPIC, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.COMPONENT_KNUCKLE_VARMOD_BALLAS, "Schlagring 'Der Ballas' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung auf einen Schlagring gezogen werden.", Rarity.EPIC, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.COMPONENT_KNUCKLE_VARMOD_DOLLAR, "Schlagring 'Dollar' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung auf einen Schlagring gezogen werden.", Rarity.EPIC, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.COMPONENT_KNUCKLE_VARMOD_DIAMOND, "Schlagring 'Diamond' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung auf einen Schlagring gezogen werden.", Rarity.EPIC, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.COMPONENT_KNUCKLE_VARMOD_HATE, "Schlagring 'Hass' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung auf einen Schlagring gezogen werden.", Rarity.EPIC, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.COMPONENT_KNUCKLE_VARMOD_LOVE, "Schlagring 'Liebe' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung auf einen Schlagring gezogen werden.", Rarity.EPIC, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.COMPONENT_KNUCKLE_VARMOD_PLAYER, "Schlagring 'Player' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung auf einen Schlagring gezogen werden.", Rarity.EPIC, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.COMPONENT_KNUCKLE_VARMOD_KING, "Schlagring 'König' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung auf einen Schlagring gezogen werden.", Rarity.EPIC, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.COMPONENT_KNUCKLE_VARMOD_VAGOS, "Schlagring 'Vagos' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung auf einen Schlagring gezogen werden.", Rarity.EPIC, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.COMPONENT_SWITCHBLADE_VARMOD_VAR1, "Springmesser 'VIP' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung auf ein Springmesser gezogen werden.", Rarity.EPIC, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.COMPONENT_SWITCHBLADE_VARMOD_VAR2, "Springmesser 'Bodybuard' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung auf ein Springmesser gezogen werden.", Rarity.EPIC, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.COMPONENT_PISTOL_VARMOD_LUXE, "Pistole 'Yusuf Amir Luxury' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung auf eine Pistole gezogen werden.", Rarity.LEGENDARY, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.COMPONENT_COMBATPISTOL_VARMOD_LOWRIDER, "Gefechtspistole 'Yusuf Amir Luxur' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung auf eine Gefechtspistole gezogen werden.", Rarity.LEGENDARY, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.COMPONENT_APPISTOL_VARMOD_LUXE, "AP Pistole 'Gilded Gun Metal' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung auf eine AP Pistole gezogen werden.", Rarity.LEGENDARY, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.COMPONENT_PISTOL50_VARMOD_LUXE, "Pistole .50 'Platinum Pearl Deluxe' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung auf eine Pistole .50 gezogen werden.", Rarity.LEGENDARY, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.COMPONENT_REVOLVER_VARMOD_BOSS, "Schwerer Revolver 'VIP' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung auf einen schweren Revolver gezogen werden.", Rarity.LEGENDARY, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.COMPONENT_REVOLVER_VARMOD_GOON, "Schwerer Revolver 'Bodyguard' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung auf einen schweren Revolver gezogen werden.", Rarity.LEGENDARY, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.COMPONENT_SNSPISTOL_VARMOD_LOWRIDER, "Billigknarre 'Etched Wood Grip' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung auf eine SNS Pistole gezogen werden.", Rarity.LEGENDARY, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.COMPONENT_HEAVYPISTOL_VARMOD_LUXE, "Schwere Pistole 'Etched Wood Grip' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung auf eine schwere Pistole gezogen werden.", Rarity.LEGENDARY, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.COMPONENT_MICROSMG_VARMOD_LUXE, "Mikro-SMG 'Yusuf Amir Luxur' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung auf eine Mikro-SMG gezogen werden.", Rarity.LEGENDARY, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.COMPONENT_SMG_VARMOD_LUXE, "SMG 'Yusuf Amir Luxur' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung auf eine SMG gezogen werden.", Rarity.LEGENDARY, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.COMPONENT_ASSAULTSMG_VARMOD_LOWRIDER, "Sturm-SMG 'Yusuf Amir Luxur' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung auf eine Sturm-SMG gezogen werden.", Rarity.LEGENDARY, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.COMPONENT_PUMPSHOTGUN_VARMOD_LOWRIDER, "Pump Schrotflinte 'Yusuf Amir Luxur' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung auf eine Pump Schrotflinte gezogen werden.", Rarity.LEGENDARY, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.COMPONENT_SAWNOFFSHOTGUN_VARMOD_LUXE, "Abgesägte Schrotflinte 'Yusuf Amir Luxur' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung auf eine abgesägte Schrotflinte gezogen werden.", Rarity.LEGENDARY, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.COMPONENT_ASSAULTRIFLE_VARMOD_LUXE, "Sturmgewehr 'Yusuf Amir Luxur' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung auf ein Sturmgewehr gezogen werden.", Rarity.LEGENDARY, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.COMPONENT_CARBINERIFLE_VARMOD_LUXE, "Karabiner 'Yusuf Amir Luxur' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung auf ein Karabiner gezogen werden.", Rarity.LEGENDARY, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.COMPONENT_ADVANCEDRIFLE_VARMOD_LUXE, "Kampfgewehr 'Yusuf Amir Luxur' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung auf ein Kampfgewehr gezogen werden.", Rarity.LEGENDARY, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.COMPONENT_SPECIALCARBINE_VARMOD_LOWRIDER, "Spezial-Karabiner 'Yusuf Amir Luxur' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung auf ein Spezial-Karabiner gezogen werden.", Rarity.LEGENDARY, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.COMPONENT_BULLPUPRIFLE_VARMOD_LOW, "Bullpup-Gewehr 'Gilded Gun Metal' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung auf ein Bullpup-Gewehr gezogen werden.", Rarity.LEGENDARY, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.COMPONENT_MG_VARMOD_LOWRIDER, "MG 'Yusuf Amir Luxur' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung auf ein MG gezogen werden.", Rarity.LEGENDARY, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.COMPONENT_COMBATMG_VARMOD_LOWRIDER, "Gefechts-MG 'Etched Gun Metal' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung auf ein Gefechts-MG gezogen werden.", Rarity.LEGENDARY, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.COMPONENT_SNIPERRIFLE_VARMOD_LUXE, "Scharfschützengewehr 'Etched Wood Grip' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung auf ein Scharfschützengewehr gezogen werden.", Rarity.LEGENDARY, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.COMPONENT_MARKSMANRIFLE_VARMOD_LUXE, "Präzisionsgewehr 'Yusuf Amir Luxury' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung auf ein Präzisionsgewehr gezogen werden.", Rarity.LEGENDARY, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.COMPONENT_PISTOL_EXTENDED_CLIP, "Handfeuerwaffe 'Erweitertes Magazin' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung auf Handfauerwaffen gezogen werden.", Rarity.EPIC, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.COMPONENT_PISTOL_FLASHLIGHT, "Handfeuerwaffe 'Taschenlampe' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung auf eine Handfauerwaffe gezogen werden.", Rarity.COMMON, 1.0f, false, false, true, false, 400, 0),
                new(ItemCatalogIds.COMPONENT_PISTOL_SUPPRESSOR, "Handfeuerwaffe 'Schalldämpfer' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung auf eine Handfauerwaffe gezogen werden.", Rarity.COMMON, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.COMPONENT_SMG_EXTENDED_CLIP, "SMGs 'Erweitertes Magazin' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung auf eine Handfauerwaffe gezogen werden.", Rarity.COMMON, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.COMPONENT_SMG_FLASHLIGHT, "SMG 'Taschenlampe' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung auf eine SMG gezogen werden.", Rarity.COMMON, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.COMPONENT_SMG_SUPPRESSOR, "SMG 'Schalldämpfer' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung auf eine SMG gezogen werden.", Rarity.COMMON, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.COMPONENT_SMG_SCOPE1, "SMG 'Visier' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung auf SMGs gezogen werden.", Rarity.COMMON, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.COMPONENT_SHOTGUN_FLASHLIGHT, "Schrotflinte 'Taschenlampe' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung auf eine Schrotflinte gezogen werden.", Rarity.COMMON, 1.0f, false, false, true, false, 600, 0),
                new(ItemCatalogIds.COMPONENT_SHOTGUN_SUPPRESSOR, "Schrotflinte 'Schalldämpfer' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung auf eine Schrotflinte gezogen werden.", Rarity.COMMON, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.COMPONENT_SHOTGUN_EXTENDED_CLIP, "Schrotflinte 'Erweitertes Magazin' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung auf eine Schrotflinte gezogen werden.", Rarity.COMMON, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.COMPONENT_RIFLE_EXTENDED_CLIP, "Gewehr 'Erweitertes Magazin' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung auf ein Gewehr gezogen werden.", Rarity.COMMON, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.COMPONENT_RIFLE_FLASHLIGHT, "Gewehr 'Taschenlampe' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung ein Gewehr gezogen werden.", Rarity.COMMON, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.COMPONENT_RIFLE_SUPPRESSOR, "Gewehr 'Schalldämpfer' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung ein Gewehr gezogen werden.", Rarity.COMMON, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.COMPONENT_RIFLE_SCOPE1, "Gewehr 'Visier' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung auf ein Gewehr gezogen werden.", Rarity.COMMON, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.COMPONENT_RIFLE_GRIP, "Gewehr 'Grip' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung auf ein Gewehr gezogen werden.", Rarity.COMMON, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.COMPONENT_MG_EXTENDED_CLIP, "MG 'Erweitertes Magazin' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung auf ein MG gezogen werden.", Rarity.COMMON, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.COMPONENT_MG_SCOPE1, "MG 'Taschenlampe' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung ein MG gezogen werden.", Rarity.COMMON, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.COMPONENT_MG_GRIP, "MG 'Schalldämpfer' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung ein MG gezogen werden.", Rarity.COMMON, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.COMPONENT_SNIPER_EXTENDED_CLIP, "Scharfschützengewehr 'Erweitertes Magazin' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung auf ein Scharfschützengewehr gezogen werden.", Rarity.COMMON, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.COMPONENT_SNIPER_FLASHLIGHT, "Scharfschützengewehr 'Taschenlampe' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung ein Scharfschützengewehr gezogen werden.", Rarity.COMMON, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.COMPONENT_SNIPER_SUPPRESSOR, "Scharfschützengewehr 'Schalldämpfer' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung ein Scharfschützengewehr gezogen werden.", Rarity.COMMON, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.COMPONENT_SNIPER_SCOPE1, "Scharfschützengewehr 'Visier I' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung auf ein Scharfschützengewehr gezogen werden.", Rarity.COMMON, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.COMPONENT_SNIPER_SCOPE2, "Scharfschützengewehr 'Visier II' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung auf ein Scharfschützengewehr gezogen werden.", Rarity.COMMON, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.COMPONENT_SNIPER_GRIP, "Scharfschützengewehr 'Grip' Erweiterung", "prop_champ_box_01", Rotation.Zero, 0, "puzzle", "Kann als Erweiterung auf ein Scharfschützengewehr gezogen werden.", Rarity.COMMON, 1.0f, false, false, false, false, 0, 0),
                new(ItemCatalogIds.DRUG_MARIJUANA, "Marihuana", "hei_prop_hei_drug_pack_01a", Rotation.Zero, 0, "marijuana", "Marihuana bezeichnet die getrockneten, harzhaltigen Blüten und die blütennahen, kleinen Blätter der weiblichen Hanfpflanze.", Rarity.RARE, 0.1f, false, true, false, false, 0, 0),
                new(ItemCatalogIds.DRUG_COCAINE, "Kokain", "hei_prop_hei_drug_pack_01a", Rotation.Zero, 0, "cocaine", "Kokain ist ein starkes Stimulans und Betäubungsmittel. Es findet weltweit Anwendung als Rauschdroge mit hohem psychischen, aber keinem physischen Abhängigkeitspotenzial.", Rarity.RARE, 0.1f, false, true, false, false, 0, 0),
                new(ItemCatalogIds.DRUG_MDMA, "MDMA", "hei_prop_hei_drug_pack_01a", Rotation.Zero, 0, "pills", "MDMA steht für eine chemische Verbindung, es gehört strukturell zur Gruppe der Methylendioxyamphetamine und ist insbesondere als weltweit verbreitete Partydroge bekannt.", Rarity.RARE, 0.1f, false, true, false, false, 0, 0),
                new(ItemCatalogIds.DRUG_XANAX, "Xanax", "hei_prop_hei_drug_pack_01a", Rotation.Zero, 0, "pills", "Alprazolam ist ein Arzneistoff aus der Gruppe der Benzodiazepine mit mittlerer Wirkungsdauer, der zur kurzzeitigen Behandlung von Angst- und Panikstörungen eingesetzt wird.", Rarity.RARE, 0.1f, false, true, false, false, 0, 0),
                new(ItemCatalogIds.DRUG_CODEINE, "Kodein", "hei_prop_hei_drug_pack_01a", Rotation.Zero, 0, "pills", "Kodein ist eine natürlich vorkommende chemische Verbindung aus der Gruppe der Opiate. In der Medizin wird Codein als Arzneistoff eingesetzt.", Rarity.RARE, 0.1f, false, true, false, false, 0, 0),
                new(ItemCatalogIds.DRUG_METH, "Meth", "hei_prop_hei_drug_pack_01a", Rotation.Zero, 0, "crystal-meth", "Methamphetamin ist eine synthetisch hergestellte Substanz aus der Stoffgruppe der Phenylethylamine. Sie wird sowohl in der Medizin als Arzneistoff wie auch als euphorisierende und stimulierende Rauschdroge verwendet.",Rarity.RARE, 0.1f, false, true, false, false, 0, 0),
            });

            var basePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data");
            var sqlFiles = Directory.GetFiles(basePath, "*.sql", SearchOption.AllDirectories);
            foreach (var sqlFile in sqlFiles)
            {
                var sql = await File.ReadAllTextAsync(sqlFile);
                await dbContext.Database.ExecuteSqlRawAsync(sql);
            }

            await dbContext.SaveChangesAsync();

            _logger.LogInformation("Seed default data.");
        }

        await Task.CompletedTask;
    }
}