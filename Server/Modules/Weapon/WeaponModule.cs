using System;
using System.Linq;
using System.Text;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using Microsoft.Extensions.Logging;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Database.Enums;
using Server.Modules.Inventory;

namespace Server.Modules.Weapon;

public class WeaponModule : ITransientScript
{
    private readonly InventoryModule _inventoryModule;
    private readonly ILogger<WeaponModule> _logger;

    public WeaponModule(ILogger<WeaponModule> logger, InventoryModule inventoryModule)
    {
        _logger = logger;
        _inventoryModule = inventoryModule;
    }

    public static bool IsThrowableWeapon(WeaponModel weaponModel)
    {
        switch (weaponModel)
        {
            case WeaponModel.Grenade:
            case WeaponModel.BZGas:
            case WeaponModel.MolotovCocktail:
            case WeaponModel.StickyBomb:
            case WeaponModel.ProximityMines:
            case WeaponModel.Snowballs:
            case WeaponModel.PipeBombs:
            case WeaponModel.Baseball:
            case WeaponModel.TearGas:
            case WeaponModel.Flare:
                return true;
            default:
                return false;
        }
    }

    public static ItemCatalogIds GetItemIdFromModel(WeaponModel weaponModel)
    {
        switch (weaponModel)
        {
            case WeaponModel.AntiqueCavalryDagger:
                return ItemCatalogIds.WEAPON_ANTIQUE_CAVALRY_DAGGER;
            case WeaponModel.BaseballBat:
                return ItemCatalogIds.WEAPON_BASEBALL_BAT;
            case WeaponModel.BrokenBottle:
                return ItemCatalogIds.WEAPON_BROKEN_BOTTLE;
            case WeaponModel.Crowbar:
                return ItemCatalogIds.WEAPON_CROWBAR;
            case WeaponModel.Flashlight:
                return ItemCatalogIds.WEAPON_FLASHLIGHT;
            case WeaponModel.GolfClub:
                return ItemCatalogIds.WEAPON_GOLF_CLUB;
            case WeaponModel.Hammer:
                return ItemCatalogIds.WEAPON_HAMMER;
            case WeaponModel.Hatchet:
                return ItemCatalogIds.WEAPON_HATCHET;
            case WeaponModel.BrassKnuckles:
                return ItemCatalogIds.WEAPON_BRASS_KNUCKLES;
            case WeaponModel.Knife:
                return ItemCatalogIds.WEAPON_KNIFE;
            case WeaponModel.Machete:
                return ItemCatalogIds.WEAPON_MACHETE;
            case WeaponModel.Switchblade:
                return ItemCatalogIds.WEAPON_SWITCHBLADE;
            case WeaponModel.Nightstick:
                return ItemCatalogIds.WEAPON_NIGHTSTICK;
            case WeaponModel.PipeWrench:
                return ItemCatalogIds.WEAPON_PIPE_WRENCH;
            case WeaponModel.BattleAxe:
                return ItemCatalogIds.WEAPON_BATTLE_AXE;
            case WeaponModel.PoolCue:
                return ItemCatalogIds.WEAPON_POOL_CUE;
            case WeaponModel.StoneHatchet:
                return ItemCatalogIds.WEAPON_STONE_HATCHET;
            case WeaponModel.Pistol:
                return ItemCatalogIds.WEAPON_PISTOL;
            case WeaponModel.PistolMkII:
                return ItemCatalogIds.WEAPON_PISTOL_MK_II;
            case WeaponModel.CombatPistol:
                return ItemCatalogIds.WEAPON_COMBAT_PISTOL;
            case WeaponModel.APPistol:
                return ItemCatalogIds.WEAPON_AP_PISTOL;
            case WeaponModel.StunGun:
                return ItemCatalogIds.WEAPON_STUN_GUN;
            case WeaponModel.Pistol50:
                return ItemCatalogIds.WEAPON_PISTOL50;
            case WeaponModel.SNSPistol:
                return ItemCatalogIds.WEAPON_SNS_PISTOL;
            case WeaponModel.SNSPistolMkII:
                return ItemCatalogIds.WEAPON_SNS_PISTOL_MK_II;
            case WeaponModel.HeavyPistol:
                return ItemCatalogIds.WEAPON_HEAVY_PISTOL;
            case WeaponModel.VintagePistol:
                return ItemCatalogIds.WEAPON_VINTAGE_PISTOL;
            case WeaponModel.FlareGun:
                return ItemCatalogIds.WEAPON_FLARE_GUN;
            case WeaponModel.MarksmanPistol:
                return ItemCatalogIds.WEAPON_MARKSMAN_PISTOL;
            case WeaponModel.HeavyRevolver:
                return ItemCatalogIds.WEAPON_HEAVY_REVOLVER;
            case WeaponModel.HeavyRevolverMkII:
                return ItemCatalogIds.WEAPON_HEAVY_REVOLVER_MK_II;
            case WeaponModel.DoubleActionRevolver:
                return ItemCatalogIds.WEAPON_DOUBLE_ACTION_REVOLVER;
            case WeaponModel.MicroSMG:
                return ItemCatalogIds.WEAPON_MICRO_SMG;
            case WeaponModel.SMG:
                return ItemCatalogIds.WEAPON_SMG;
            case WeaponModel.SMGMkII:
                return ItemCatalogIds.WEAPON_SMG_MK_II;
            case WeaponModel.AssaultSMG:
                return ItemCatalogIds.WEAPON_ASSAULT_SMG;
            case WeaponModel.CombatPDW:
                return ItemCatalogIds.WEAPON_COMBAT_PDW;
            case WeaponModel.MachinePistol:
                return ItemCatalogIds.WEAPON_MACHINE_PISTOL;
            case WeaponModel.MiniSMG:
                return ItemCatalogIds.WEAPON_MINI_SMG;
            case WeaponModel.PumpShotgun:
                return ItemCatalogIds.WEAPON_PUMP_SHOTGUN;
            case WeaponModel.PumpShotgunMkII:
                return ItemCatalogIds.WEAPON_PUMP_SHOTGUN_MK_II;
            case WeaponModel.SawedOffShotgun:
                return ItemCatalogIds.WEAPON_SAWED_OFF_SHOTGUN;
            case WeaponModel.AssaultShotgun:
                return ItemCatalogIds.WEAPON_ASSAULT_SHOTGUN;
            case WeaponModel.BullpupShotgun:
                return ItemCatalogIds.WEAPON_BULLPUP_SHOTGUN;
            case WeaponModel.Musket:
                return ItemCatalogIds.WEAPON_MUSKET;
            case WeaponModel.HeavyShotgun:
                return ItemCatalogIds.WEAPON_HEAVY_SHOTGUN;
            case WeaponModel.DoubleBarrelShotgun:
                return ItemCatalogIds.WEAPON_DOUBLE_BARREL_SHOTGUN;
            case WeaponModel.SweeperShotgun:
                return ItemCatalogIds.WEAPON_SWEEPER_SHOTGUN;
            case WeaponModel.AssaultRifle:
                return ItemCatalogIds.WEAPON_ASSAULT_RIFLE;
            case WeaponModel.AssaultRifleMkII:
                return ItemCatalogIds.WEAPON_ASSAULT_RIFLE_MK_II;
            case WeaponModel.CarbineRifle:
                return ItemCatalogIds.WEAPON_CARBINE_RIFLE;
            case WeaponModel.CarbineRifleMkII:
                return ItemCatalogIds.WEAPON_CARBINE_RIFLE_MK_II;
            case WeaponModel.AdvancedRifle:
                return ItemCatalogIds.WEAPON_ADVANCED_RIFLE;
            case WeaponModel.SpecialCarbine:
                return ItemCatalogIds.WEAPON_SPECIAL_CARBINE;
            case WeaponModel.SpecialCarbineMkII:
                return ItemCatalogIds.WEAPON_SPECIAL_CARBINE_MK_II;
            case WeaponModel.BullpupRifle:
                return ItemCatalogIds.WEAPON_BULLPUP_RIFLE;
            case WeaponModel.BullpupRifleMkII:
                return ItemCatalogIds.WEAPON_BULLPUP_RIFLE_MK_II;
            case WeaponModel.CompactRifle:
                return ItemCatalogIds.WEAPON_COMPACT_RIFLE;
            case WeaponModel.MG:
                return ItemCatalogIds.WEAPON_MG;
            case WeaponModel.CombatMG:
                return ItemCatalogIds.WEAPON_COMBAT_MG;
            case WeaponModel.CombatMGMkII:
                return ItemCatalogIds.WEAPON_COMBAT_MG_MK_II;
            case WeaponModel.GusenbergSweeper:
                return ItemCatalogIds.WEAPON_GUSENBERG_SWEEPER;
            case WeaponModel.SniperRifle:
                return ItemCatalogIds.WEAPON_SNIPER_RIFLE;
            case WeaponModel.HeavySniper:
                return ItemCatalogIds.WEAPON_HEAVY_SNIPER;
            case WeaponModel.HeavySniperMkII:
                return ItemCatalogIds.WEAPON_HEAVY_SNIPER_MK_II;
            case WeaponModel.MarksmanRifle:
                return ItemCatalogIds.WEAPON_MARKSMAN_RIFLE;
            case WeaponModel.MarksmanRifleMkII:
                return ItemCatalogIds.WEAPON_MARKSMAN_RIFLE_MK_II;
            case WeaponModel.Grenade:
                return ItemCatalogIds.WEAPON_GRENADE;
            case WeaponModel.BZGas:
                return ItemCatalogIds.WEAPON_BZ_GAS;
            case WeaponModel.MolotovCocktail:
                return ItemCatalogIds.WEAPON_MOLOTOV_COCKTAIL;
            case WeaponModel.Snowballs:
                return ItemCatalogIds.WEAPON_SNOWBALL;
            case WeaponModel.Baseball:
                return ItemCatalogIds.WEAPON_BASEBALL;
            case WeaponModel.Flare:
                return ItemCatalogIds.WEAPON_FLARE;
            case WeaponModel.JerryCan:
                return ItemCatalogIds.WEAPON_JERRY_CAN;
            case WeaponModel.Parachute:
                return ItemCatalogIds.WEAPON_PARACHUTE;
            case WeaponModel.FireExtinguisher:
                return ItemCatalogIds.WEAPON_FIRE_EXTINGUISHER;
            case WeaponModel.MilitaryRifle:
                return ItemCatalogIds.WEAPON_MILITARY_RIFLE;
            case WeaponModel.CombatShotgun:
                return ItemCatalogIds.WEAPON_COMBAT_SHOTGUN;
            default:
                throw new ArgumentOutOfRangeException(nameof(weaponModel), weaponModel, null);
        }
    }

    public static WeaponModel GetModelFromId(ItemCatalogIds itemCatalogId)
    {
        switch (itemCatalogId)
        {
            case ItemCatalogIds.WEAPON_ANTIQUE_CAVALRY_DAGGER:
                return WeaponModel.AntiqueCavalryDagger;
            case ItemCatalogIds.WEAPON_BASEBALL_BAT:
                return WeaponModel.BaseballBat;
            case ItemCatalogIds.WEAPON_BROKEN_BOTTLE:
                return WeaponModel.BrokenBottle;
            case ItemCatalogIds.WEAPON_CROWBAR:
                return WeaponModel.Crowbar;
            case ItemCatalogIds.WEAPON_FLASHLIGHT:
                return WeaponModel.Flashlight;
            case ItemCatalogIds.WEAPON_GOLF_CLUB:
                return WeaponModel.GolfClub;
            case ItemCatalogIds.WEAPON_HAMMER:
                return WeaponModel.Hammer;
            case ItemCatalogIds.WEAPON_HATCHET:
                return WeaponModel.Hatchet;
            case ItemCatalogIds.WEAPON_BRASS_KNUCKLES:
                return WeaponModel.BrassKnuckles;
            case ItemCatalogIds.WEAPON_KNIFE:
                return WeaponModel.Knife;
            case ItemCatalogIds.WEAPON_MACHETE:
                return WeaponModel.Machete;
            case ItemCatalogIds.WEAPON_SWITCHBLADE:
                return WeaponModel.Switchblade;
            case ItemCatalogIds.WEAPON_NIGHTSTICK:
                return WeaponModel.Nightstick;
            case ItemCatalogIds.WEAPON_PIPE_WRENCH:
                return WeaponModel.PipeWrench;
            case ItemCatalogIds.WEAPON_BATTLE_AXE:
                return WeaponModel.BattleAxe;
            case ItemCatalogIds.WEAPON_POOL_CUE:
                return WeaponModel.PoolCue;
            case ItemCatalogIds.WEAPON_STONE_HATCHET:
                return WeaponModel.StoneHatchet;
            case ItemCatalogIds.WEAPON_PISTOL:
                return WeaponModel.Pistol;
            case ItemCatalogIds.WEAPON_PISTOL_MK_II:
                return WeaponModel.PistolMkII;
            case ItemCatalogIds.WEAPON_COMBAT_PISTOL:
                return WeaponModel.CombatPistol;
            case ItemCatalogIds.WEAPON_AP_PISTOL:
                return WeaponModel.APPistol;
            case ItemCatalogIds.WEAPON_STUN_GUN:
                return WeaponModel.StunGun;
            case ItemCatalogIds.WEAPON_PISTOL50:
                return WeaponModel.Pistol50;
            case ItemCatalogIds.WEAPON_SNS_PISTOL:
                return WeaponModel.SNSPistol;
            case ItemCatalogIds.WEAPON_SNS_PISTOL_MK_II:
                return WeaponModel.SNSPistolMkII;
            case ItemCatalogIds.WEAPON_HEAVY_PISTOL:
                return WeaponModel.HeavyPistol;
            case ItemCatalogIds.WEAPON_VINTAGE_PISTOL:
                return WeaponModel.VintagePistol;
            case ItemCatalogIds.WEAPON_FLARE_GUN:
                return WeaponModel.FlareGun;
            case ItemCatalogIds.WEAPON_MARKSMAN_PISTOL:
                return WeaponModel.MarksmanPistol;
            case ItemCatalogIds.WEAPON_HEAVY_REVOLVER:
                return WeaponModel.HeavyRevolver;
            case ItemCatalogIds.WEAPON_HEAVY_REVOLVER_MK_II:
                return WeaponModel.HeavyRevolverMkII;
            case ItemCatalogIds.WEAPON_DOUBLE_ACTION_REVOLVER:
                return WeaponModel.DoubleActionRevolver;
            case ItemCatalogIds.WEAPON_MICRO_SMG:
                return WeaponModel.MicroSMG;
            case ItemCatalogIds.WEAPON_SMG:
                return WeaponModel.SMG;
            case ItemCatalogIds.WEAPON_SMG_MK_II:
                return WeaponModel.SMGMkII;
            case ItemCatalogIds.WEAPON_ASSAULT_SMG:
                return WeaponModel.AssaultSMG;
            case ItemCatalogIds.WEAPON_COMBAT_PDW:
                return WeaponModel.CombatPDW;
            case ItemCatalogIds.WEAPON_MACHINE_PISTOL:
                return WeaponModel.MachinePistol;
            case ItemCatalogIds.WEAPON_MINI_SMG:
                return WeaponModel.MiniSMG;
            case ItemCatalogIds.WEAPON_PUMP_SHOTGUN:
                return WeaponModel.PumpShotgun;
            case ItemCatalogIds.WEAPON_PUMP_SHOTGUN_MK_II:
                return WeaponModel.PumpShotgunMkII;
            case ItemCatalogIds.WEAPON_SAWED_OFF_SHOTGUN:
                return WeaponModel.SawedOffShotgun;
            case ItemCatalogIds.WEAPON_ASSAULT_SHOTGUN:
                return WeaponModel.AssaultShotgun;
            case ItemCatalogIds.WEAPON_BULLPUP_SHOTGUN:
                return WeaponModel.BullpupShotgun;
            case ItemCatalogIds.WEAPON_MUSKET:
                return WeaponModel.Musket;
            case ItemCatalogIds.WEAPON_HEAVY_SHOTGUN:
                return WeaponModel.HeavyShotgun;
            case ItemCatalogIds.WEAPON_DOUBLE_BARREL_SHOTGUN:
                return WeaponModel.DoubleBarrelShotgun;
            case ItemCatalogIds.WEAPON_SWEEPER_SHOTGUN:
                return WeaponModel.SweeperShotgun;
            case ItemCatalogIds.WEAPON_ASSAULT_RIFLE:
                return WeaponModel.AssaultRifle;
            case ItemCatalogIds.WEAPON_ASSAULT_RIFLE_MK_II:
                return WeaponModel.AssaultRifleMkII;
            case ItemCatalogIds.WEAPON_CARBINE_RIFLE:
                return WeaponModel.CarbineRifle;
            case ItemCatalogIds.WEAPON_CARBINE_RIFLE_MK_II:
                return WeaponModel.CarbineRifleMkII;
            case ItemCatalogIds.WEAPON_ADVANCED_RIFLE:
                return WeaponModel.AdvancedRifle;
            case ItemCatalogIds.WEAPON_SPECIAL_CARBINE:
                return WeaponModel.SpecialCarbine;
            case ItemCatalogIds.WEAPON_SPECIAL_CARBINE_MK_II:
                return WeaponModel.SpecialCarbineMkII;
            case ItemCatalogIds.WEAPON_BULLPUP_RIFLE:
                return WeaponModel.BullpupRifle;
            case ItemCatalogIds.WEAPON_BULLPUP_RIFLE_MK_II:
                return WeaponModel.BullpupRifleMkII;
            case ItemCatalogIds.WEAPON_COMPACT_RIFLE:
                return WeaponModel.CompactRifle;
            case ItemCatalogIds.WEAPON_MG:
                return WeaponModel.MG;
            case ItemCatalogIds.WEAPON_COMBAT_MG:
                return WeaponModel.CombatMG;
            case ItemCatalogIds.WEAPON_COMBAT_MG_MK_II:
                return WeaponModel.CombatMGMkII;
            case ItemCatalogIds.WEAPON_GUSENBERG_SWEEPER:
                return WeaponModel.GusenbergSweeper;
            case ItemCatalogIds.WEAPON_SNIPER_RIFLE:
                return WeaponModel.SniperRifle;
            case ItemCatalogIds.WEAPON_HEAVY_SNIPER:
                return WeaponModel.HeavySniper;
            case ItemCatalogIds.WEAPON_HEAVY_SNIPER_MK_II:
                return WeaponModel.HeavySniperMkII;
            case ItemCatalogIds.WEAPON_MARKSMAN_RIFLE:
                return WeaponModel.MarksmanRifle;
            case ItemCatalogIds.WEAPON_MARKSMAN_RIFLE_MK_II:
                return WeaponModel.MarksmanRifleMkII;
            case ItemCatalogIds.WEAPON_GRENADE:
                return WeaponModel.Grenade;
            case ItemCatalogIds.WEAPON_BZ_GAS:
                return WeaponModel.BZGas;
            case ItemCatalogIds.WEAPON_MOLOTOV_COCKTAIL:
                return WeaponModel.MolotovCocktail;
            case ItemCatalogIds.WEAPON_SNOWBALL:
                return WeaponModel.Snowballs;
            case ItemCatalogIds.WEAPON_BASEBALL:
                return WeaponModel.Baseball;
            case ItemCatalogIds.WEAPON_FLARE:
                return WeaponModel.Flare;
            case ItemCatalogIds.WEAPON_JERRY_CAN:
                return WeaponModel.JerryCan;
            case ItemCatalogIds.WEAPON_PARACHUTE:
                return WeaponModel.Parachute;
            case ItemCatalogIds.WEAPON_FIRE_EXTINGUISHER:
                return WeaponModel.FireExtinguisher;
            case ItemCatalogIds.WEAPON_MILITARY_RIFLE:
                return WeaponModel.MilitaryRifle;
            case ItemCatalogIds.WEAPON_COMBAT_SHOTGUN:
                return WeaponModel.CombatShotgun;
            default:
                throw new ArgumentOutOfRangeException(nameof(itemCatalogId), itemCatalogId, null);
        }
    }

    public static bool IsItemWeapon(ItemCatalogIds itemCatalogId)
    {
        switch (itemCatalogId)
        {
            case ItemCatalogIds.WEAPON_ANTIQUE_CAVALRY_DAGGER:
            case ItemCatalogIds.WEAPON_BASEBALL_BAT:
            case ItemCatalogIds.WEAPON_BROKEN_BOTTLE:
            case ItemCatalogIds.WEAPON_CROWBAR:
            case ItemCatalogIds.WEAPON_FLASHLIGHT:
            case ItemCatalogIds.WEAPON_GOLF_CLUB:
            case ItemCatalogIds.WEAPON_HAMMER:
            case ItemCatalogIds.WEAPON_HATCHET:
            case ItemCatalogIds.WEAPON_BRASS_KNUCKLES:
            case ItemCatalogIds.WEAPON_KNIFE:
            case ItemCatalogIds.WEAPON_MACHETE:
            case ItemCatalogIds.WEAPON_SWITCHBLADE:
            case ItemCatalogIds.WEAPON_NIGHTSTICK:
            case ItemCatalogIds.WEAPON_PIPE_WRENCH:
            case ItemCatalogIds.WEAPON_BATTLE_AXE:
            case ItemCatalogIds.WEAPON_POOL_CUE:
            case ItemCatalogIds.WEAPON_STONE_HATCHET:
            case ItemCatalogIds.WEAPON_PISTOL:
            case ItemCatalogIds.WEAPON_PISTOL_MK_II:
            case ItemCatalogIds.WEAPON_COMBAT_PISTOL:
            case ItemCatalogIds.WEAPON_AP_PISTOL:
            case ItemCatalogIds.WEAPON_STUN_GUN:
            case ItemCatalogIds.WEAPON_PISTOL50:
            case ItemCatalogIds.WEAPON_SNS_PISTOL:
            case ItemCatalogIds.WEAPON_SNS_PISTOL_MK_II:
            case ItemCatalogIds.WEAPON_HEAVY_PISTOL:
            case ItemCatalogIds.WEAPON_VINTAGE_PISTOL:
            case ItemCatalogIds.WEAPON_FLARE_GUN:
            case ItemCatalogIds.WEAPON_MARKSMAN_PISTOL:
            case ItemCatalogIds.WEAPON_HEAVY_REVOLVER:
            case ItemCatalogIds.WEAPON_HEAVY_REVOLVER_MK_II:
            case ItemCatalogIds.WEAPON_DOUBLE_ACTION_REVOLVER:
            case ItemCatalogIds.WEAPON_MICRO_SMG:
            case ItemCatalogIds.WEAPON_SMG:
            case ItemCatalogIds.WEAPON_SMG_MK_II:
            case ItemCatalogIds.WEAPON_ASSAULT_SMG:
            case ItemCatalogIds.WEAPON_COMBAT_PDW:
            case ItemCatalogIds.WEAPON_MACHINE_PISTOL:
            case ItemCatalogIds.WEAPON_MINI_SMG:
            case ItemCatalogIds.WEAPON_PUMP_SHOTGUN:
            case ItemCatalogIds.WEAPON_PUMP_SHOTGUN_MK_II:
            case ItemCatalogIds.WEAPON_SAWED_OFF_SHOTGUN:
            case ItemCatalogIds.WEAPON_ASSAULT_SHOTGUN:
            case ItemCatalogIds.WEAPON_BULLPUP_SHOTGUN:
            case ItemCatalogIds.WEAPON_MUSKET:
            case ItemCatalogIds.WEAPON_HEAVY_SHOTGUN:
            case ItemCatalogIds.WEAPON_DOUBLE_BARREL_SHOTGUN:
            case ItemCatalogIds.WEAPON_SWEEPER_SHOTGUN:
            case ItemCatalogIds.WEAPON_ASSAULT_RIFLE:
            case ItemCatalogIds.WEAPON_ASSAULT_RIFLE_MK_II:
            case ItemCatalogIds.WEAPON_CARBINE_RIFLE:
            case ItemCatalogIds.WEAPON_CARBINE_RIFLE_MK_II:
            case ItemCatalogIds.WEAPON_ADVANCED_RIFLE:
            case ItemCatalogIds.WEAPON_SPECIAL_CARBINE:
            case ItemCatalogIds.WEAPON_SPECIAL_CARBINE_MK_II:
            case ItemCatalogIds.WEAPON_BULLPUP_RIFLE:
            case ItemCatalogIds.WEAPON_BULLPUP_RIFLE_MK_II:
            case ItemCatalogIds.WEAPON_COMPACT_RIFLE:
            case ItemCatalogIds.WEAPON_MG:
            case ItemCatalogIds.WEAPON_COMBAT_MG:
            case ItemCatalogIds.WEAPON_COMBAT_MG_MK_II:
            case ItemCatalogIds.WEAPON_GUSENBERG_SWEEPER:
            case ItemCatalogIds.WEAPON_SNIPER_RIFLE:
            case ItemCatalogIds.WEAPON_HEAVY_SNIPER:
            case ItemCatalogIds.WEAPON_HEAVY_SNIPER_MK_II:
            case ItemCatalogIds.WEAPON_MARKSMAN_RIFLE:
            case ItemCatalogIds.WEAPON_MARKSMAN_RIFLE_MK_II:
            case ItemCatalogIds.WEAPON_GRENADE:
            case ItemCatalogIds.WEAPON_BZ_GAS:
            case ItemCatalogIds.WEAPON_MOLOTOV_COCKTAIL:
            case ItemCatalogIds.WEAPON_SNOWBALL:
            case ItemCatalogIds.WEAPON_BASEBALL:
            case ItemCatalogIds.WEAPON_FLARE:
            case ItemCatalogIds.WEAPON_JERRY_CAN:
            case ItemCatalogIds.WEAPON_PARACHUTE:
            case ItemCatalogIds.WEAPON_FIRE_EXTINGUISHER:
            case ItemCatalogIds.WEAPON_MILITARY_RIFLE:
            case ItemCatalogIds.WEAPON_COMBAT_SHOTGUN:
                return true;
            default:
                return false;
        }
    }

    public static string CreateSerialNumber()
    {
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var rnd = new Random();
        var stringBuilder = new StringBuilder(7);

        for (var i = 0; i < 2; i++)
        {
            var num = rnd.Next(0, chars.Length - 1);
            stringBuilder.Append(chars[num]);
        }

        stringBuilder.Append(rnd.Next(10000, 99999));

        return stringBuilder.ToString();
    }

    /// <summary>
    ///     Give player a gta weapon.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="weaponModel"></param>
    /// <param name="equipped"></param>
    /// <param name="amount">Use amount only for throwable weapons</param>
    public void Give(ServerPlayer player, WeaponModel weaponModel, bool equipped, int amount = 0)
    {
        if (IsThrowableWeapon(weaponModel))
        {
            player.GiveWeapon(weaponModel, amount, equipped);
            return;
        }

        player.GiveWeapon(weaponModel, 0, equipped);
    }

    /// <summary>
    ///     Will only remove the weapon when the player has no other exact same weapon item in his inventory.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="weaponModel"></param>
    public void Remove(ServerPlayer player, WeaponModel weaponModel)
    {
        var catalogItemId = GetItemIdFromModel(weaponModel);

        var amountOfWeapon =
            player.CharacterModel.InventoryModel.Items.Count(i => i.CatalogItemModelId == catalogItemId);
        if (amountOfWeapon != 1)
        {
            return;
        }

        player.RemoveWeapon(weaponModel);
    }
}