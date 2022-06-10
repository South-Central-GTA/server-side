using System.Collections.Generic;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Async;
using Microsoft.Extensions.Logging;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Inventory;
using Server.Modules.Inventory;

namespace Server.Modules.Weapon;

public class AttachmentModule : ITransientScript
{
    public static Dictionary<ItemCatalogIds, Dictionary<ItemCatalogIds, string>> ComponentTable = new()
    {
        {
            ItemCatalogIds.WEAPON_BRASS_KNUCKLES,
            new Dictionary<ItemCatalogIds, string>
            {
                { ItemCatalogIds.COMPONENT_KNUCKLE_VARMOD_PIMP, "COMPONENT_KNUCKLE_VARMOD_PIMP" },
                { ItemCatalogIds.COMPONENT_KNUCKLE_VARMOD_BALLAS, "COMPONENT_KNUCKLE_VARMOD_BALLAS" },
                { ItemCatalogIds.COMPONENT_KNUCKLE_VARMOD_DOLLAR, "COMPONENT_KNUCKLE_VARMOD_DOLLAR" },
                { ItemCatalogIds.COMPONENT_KNUCKLE_VARMOD_DIAMOND, "COMPONENT_KNUCKLE_VARMOD_DIAMOND" },
                { ItemCatalogIds.COMPONENT_KNUCKLE_VARMOD_HATE, "COMPONENT_KNUCKLE_VARMOD_HATE" },
                { ItemCatalogIds.COMPONENT_KNUCKLE_VARMOD_LOVE, "COMPONENT_KNUCKLE_VARMOD_LOVE" },
                { ItemCatalogIds.COMPONENT_KNUCKLE_VARMOD_PLAYER, "COMPONENT_KNUCKLE_VARMOD_PLAYER" },
                { ItemCatalogIds.COMPONENT_KNUCKLE_VARMOD_KING, "COMPONENT_KNUCKLE_VARMOD_KING" },
                { ItemCatalogIds.COMPONENT_KNUCKLE_VARMOD_VAGOS, "COMPONENT_KNUCKLE_VARMOD_VAGOS" }
            }
        },
        {
            ItemCatalogIds.WEAPON_SWITCHBLADE,
            new Dictionary<ItemCatalogIds, string>
            {
                { ItemCatalogIds.COMPONENT_SWITCHBLADE_VARMOD_VAR1, "COMPONENT_SWITCHBLADE_VARMOD_VAR1" },
                { ItemCatalogIds.COMPONENT_SWITCHBLADE_VARMOD_VAR2, "COMPONENT_SWITCHBLADE_VARMOD_VAR2" }
            }
        },
        {
            ItemCatalogIds.WEAPON_PISTOL,
            new Dictionary<ItemCatalogIds, string>
            {
                { ItemCatalogIds.COMPONENT_PISTOL_EXTENDED_CLIP, "COMPONENT_PISTOL_CLIP_02" },
                { ItemCatalogIds.COMPONENT_PISTOL_FLASHLIGHT, "COMPONENT_AT_PI_FLSH" },
                { ItemCatalogIds.COMPONENT_PISTOL_SUPPRESSOR, "COMPONENT_AT_PI_SUPP_02" },
                { ItemCatalogIds.COMPONENT_PISTOL_VARMOD_LUXE, "COMPONENT_PISTOL_VARMOD_LUXE" }
            }
        },
        {
            ItemCatalogIds.WEAPON_PISTOL_MK_II,
            new Dictionary<ItemCatalogIds, string>
            {
                { ItemCatalogIds.COMPONENT_PISTOL_EXTENDED_CLIP, "COMPONENT_PISTOL_MK2_CLIP_02" },
                { ItemCatalogIds.COMPONENT_PISTOL_FLASHLIGHT, "COMPONENT_AT_PI_FLSH_02" },
                { ItemCatalogIds.COMPONENT_PISTOL_SUPPRESSOR, "COMPONENT_AT_PI_SUPP_02" }
            }
        },
        {
            ItemCatalogIds.WEAPON_COMBAT_PISTOL,
            new Dictionary<ItemCatalogIds, string>
            {
                { ItemCatalogIds.COMPONENT_PISTOL_EXTENDED_CLIP, "COMPONENT_COMBATPISTOL_CLIP_02" },
                { ItemCatalogIds.COMPONENT_PISTOL_FLASHLIGHT, "COMPONENT_AT_PI_FLSH" },
                { ItemCatalogIds.COMPONENT_PISTOL_SUPPRESSOR, "COMPONENT_AT_PI_SUPP" },
                { ItemCatalogIds.COMPONENT_COMBATPISTOL_VARMOD_LOWRIDER, "COMPONENT_COMBATPISTOL_VARMOD_LOWRIDER" }
            }
        },
        {
            ItemCatalogIds.WEAPON_HEAVY_REVOLVER,
            new Dictionary<ItemCatalogIds, string>
            {
                { ItemCatalogIds.COMPONENT_REVOLVER_VARMOD_BOSS, "COMPONENT_REVOLVER_VARMOD_BOSS" },
                { ItemCatalogIds.COMPONENT_REVOLVER_VARMOD_GOON, "COMPONENT_REVOLVER_VARMOD_GOON" }
            }
        },
        {
            ItemCatalogIds.WEAPON_AP_PISTOL,
            new Dictionary<ItemCatalogIds, string>
            {
                { ItemCatalogIds.COMPONENT_PISTOL_EXTENDED_CLIP, "COMPONENT_APPISTOL_CLIP_02" },
                { ItemCatalogIds.COMPONENT_PISTOL_FLASHLIGHT, "COMPONENT_AT_PI_FLSH" },
                { ItemCatalogIds.COMPONENT_PISTOL_SUPPRESSOR, "COMPONENT_AT_PI_SUPP" },
                { ItemCatalogIds.COMPONENT_APPISTOL_VARMOD_LUXE, "COMPONENT_APPISTOL_VARMOD_LUXE" }
            }
        },
        {
            ItemCatalogIds.WEAPON_PISTOL50,
            new Dictionary<ItemCatalogIds, string>
            {
                { ItemCatalogIds.COMPONENT_PISTOL_EXTENDED_CLIP, "COMPONENT_PISTOL50_CLIP_02" },
                { ItemCatalogIds.COMPONENT_PISTOL_FLASHLIGHT, "COMPONENT_AT_PI_FLSH" },
                { ItemCatalogIds.COMPONENT_PISTOL_SUPPRESSOR, "COMPONENT_AT_AR_SUPP_02" },
                { ItemCatalogIds.COMPONENT_PISTOL50_VARMOD_LUXE, "COMPONENT_PISTOL50_VARMOD_LUXE" }
            }
        },
        {
            ItemCatalogIds.WEAPON_SNS_PISTOL,
            new Dictionary<ItemCatalogIds, string>
            {
                { ItemCatalogIds.COMPONENT_PISTOL_EXTENDED_CLIP, "COMPONENT_SNSPISTOL_CLIP_02" },
                { ItemCatalogIds.COMPONENT_SNSPISTOL_VARMOD_LOWRIDER, "COMPONENT_SNSPISTOL_VARMOD_LOWRIDER" }
            }
        },
        {
            ItemCatalogIds.WEAPON_SNS_PISTOL_MK_II,
            new Dictionary<ItemCatalogIds, string>
            {
                { ItemCatalogIds.COMPONENT_PISTOL_EXTENDED_CLIP, "COMPONENT_SNSPISTOL_MK2_CLIP_02" },
                { ItemCatalogIds.COMPONENT_PISTOL_FLASHLIGHT, "COMPONENT_AT_PI_FLSH_03" },
                { ItemCatalogIds.COMPONENT_PISTOL_SUPPRESSOR, "COMPONENT_AT_PI_SUPP_02" }
            }
        },
        {
            ItemCatalogIds.WEAPON_HEAVY_PISTOL,
            new Dictionary<ItemCatalogIds, string>
            {
                { ItemCatalogIds.COMPONENT_PISTOL_EXTENDED_CLIP, "COMPONENT_HEAVYPISTOL_CLIP_02" },
                { ItemCatalogIds.COMPONENT_PISTOL_FLASHLIGHT, "COMPONENT_AT_PI_FLSH" },
                { ItemCatalogIds.COMPONENT_PISTOL_SUPPRESSOR, "COMPONENT_AT_PI_SUPP" },
                { ItemCatalogIds.COMPONENT_HEAVYPISTOL_VARMOD_LUXE, "COMPONENT_HEAVYPISTOL_VARMOD_LUXE" }
            }
        },
        {
            ItemCatalogIds.WEAPON_VINTAGE_PISTOL,
            new Dictionary<ItemCatalogIds, string>
            {
                { ItemCatalogIds.COMPONENT_PISTOL_EXTENDED_CLIP, "COMPONENT_VINTAGEPISTOL_CLIP_02" },
                { ItemCatalogIds.COMPONENT_PISTOL_SUPPRESSOR, "COMPONENT_AT_PI_SUPP" }
            }
        },
        {
            ItemCatalogIds.WEAPON_HEAVY_REVOLVER_MK_II,
            new Dictionary<ItemCatalogIds, string>
            {
                { ItemCatalogIds.COMPONENT_PISTOL_EXTENDED_CLIP, "COMPONENT_HEAVYPISTOL_CLIP_02" }
            }
        },
        {
            ItemCatalogIds.WEAPON_MICRO_SMG,
            new Dictionary<ItemCatalogIds, string>
            {
                { ItemCatalogIds.COMPONENT_SMG_EXTENDED_CLIP, "COMPONENT_MICROSMG_CLIP_02" },
                { ItemCatalogIds.COMPONENT_SMG_FLASHLIGHT, "COMPONENT_AT_PI_FLSH" },
                { ItemCatalogIds.COMPONENT_SMG_SUPPRESSOR, "COMPONENT_AT_AR_SUPP_02" },
                { ItemCatalogIds.COMPONENT_MICROSMG_VARMOD_LUXE, "COMPONENT_MICROSMG_VARMOD_LUXE" }
            }
        },
        {
            ItemCatalogIds.WEAPON_SMG,
            new Dictionary<ItemCatalogIds, string>
            {
                { ItemCatalogIds.COMPONENT_SMG_EXTENDED_CLIP, "COMPONENT_SMG_CLIP_02" },
                { ItemCatalogIds.COMPONENT_SMG_FLASHLIGHT, "COMPONENT_AT_AR_FLSH" },
                { ItemCatalogIds.COMPONENT_SMG_SUPPRESSOR, "COMPONENT_AT_PI_SUPP" },
                { ItemCatalogIds.COMPONENT_SMG_VARMOD_LUXE, "COMPONENT_SMG_VARMOD_LUXE" }
            }
        },
        {
            ItemCatalogIds.WEAPON_ASSAULT_SMG,
            new Dictionary<ItemCatalogIds, string>
            {
                { ItemCatalogIds.COMPONENT_SMG_EXTENDED_CLIP, "COMPONENT_ASSAULTSMG_CLIP_02" },
                { ItemCatalogIds.COMPONENT_SMG_FLASHLIGHT, "COMPONENT_AT_AR_FLSH" },
                { ItemCatalogIds.COMPONENT_SMG_SUPPRESSOR, "COMPONENT_AT_AR_SUPP_02" },
                { ItemCatalogIds.COMPONENT_ASSAULTSMG_VARMOD_LOWRIDER, "COMPONENT_ASSAULTSMG_VARMOD_LOWRIDER" }
            }
        },
        {
            ItemCatalogIds.WEAPON_MINI_SMG,
            new Dictionary<ItemCatalogIds, string>
            {
                { ItemCatalogIds.COMPONENT_SMG_EXTENDED_CLIP, "COMPONENT_MINISMG_CLIP_02" }
            }
        },
        {
            ItemCatalogIds.WEAPON_SMG_MK_II,
            new Dictionary<ItemCatalogIds, string>
            {
                { ItemCatalogIds.COMPONENT_SMG_EXTENDED_CLIP, "COMPONENT_SMG_MK2_CLIP_02" },
                { ItemCatalogIds.COMPONENT_SMG_FLASHLIGHT, "COMPONENT_AT_AR_FLSH" },
                { ItemCatalogIds.COMPONENT_SMG_SUPPRESSOR, "COMPONENT_AT_PI_SUPP" }
            }
        },
        {
            ItemCatalogIds.WEAPON_MACHINE_PISTOL,
            new Dictionary<ItemCatalogIds, string>
            {
                { ItemCatalogIds.COMPONENT_SMG_EXTENDED_CLIP, "COMPONENT_MACHINEPISTOL_CLIP_02" },
                { ItemCatalogIds.COMPONENT_SMG_SUPPRESSOR, "COMPONENT_AT_PI_SUPP" }
            }
        },
        {
            ItemCatalogIds.WEAPON_COMBAT_PDW,
            new Dictionary<ItemCatalogIds, string>
            {
                { ItemCatalogIds.COMPONENT_SMG_EXTENDED_CLIP, "COMPONENT_COMBATPDW_CLIP_02" },
                { ItemCatalogIds.COMPONENT_SMG_FLASHLIGHT, "COMPONENT_AT_AR_FLSH" },
                { ItemCatalogIds.COMPONENT_SMG_GRIP, "COMPONENT_AT_AR_AFGRIP" }
            }
        },
        {
            ItemCatalogIds.WEAPON_PUMP_SHOTGUN,
            new Dictionary<ItemCatalogIds, string>
            {
                { ItemCatalogIds.COMPONENT_SHOTGUN_FLASHLIGHT, "COMPONENT_AT_AR_FLSH" },
                { ItemCatalogIds.COMPONENT_SHOTGUN_SUPPRESSOR, "COMPONENT_AT_SR_SUPP" },
                { ItemCatalogIds.COMPONENT_PUMPSHOTGUN_VARMOD_LOWRIDER, "COMPONENT_PUMPSHOTGUN_VARMOD_LOWRIDER" }
            }
        },
        {
            ItemCatalogIds.WEAPON_ASSAULT_SHOTGUN,
            new Dictionary<ItemCatalogIds, string>
            {
                { ItemCatalogIds.COMPONENT_SHOTGUN_EXTENDED_CLIP, "COMPONENT_ASSAULTSHOTGUN_CLIP_02" },
                { ItemCatalogIds.COMPONENT_SHOTGUN_FLASHLIGHT, "COMPONENT_AT_AR_FLSH" },
                { ItemCatalogIds.COMPONENT_SHOTGUN_SUPPRESSOR, "COMPONENT_AT_AR_SUPP" },
                { ItemCatalogIds.COMPONENT_SHOTGUN_GRIP, "COMPONENT_AT_AR_AFGRIP" }
            }
        },
        {
            ItemCatalogIds.COMPONENT_SAWNOFFSHOTGUN_VARMOD_LUXE,
            new Dictionary<ItemCatalogIds, string>
            {
                { ItemCatalogIds.COMPONENT_SAWNOFFSHOTGUN_VARMOD_LUXE, "COMPONENT_SAWNOFFSHOTGUN_VARMOD_LUXE" }
            }
        },
        {
            ItemCatalogIds.WEAPON_BULLPUP_SHOTGUN,
            new Dictionary<ItemCatalogIds, string>
            {
                { ItemCatalogIds.COMPONENT_SHOTGUN_FLASHLIGHT, "COMPONENT_AT_AR_FLSH" },
                { ItemCatalogIds.COMPONENT_SHOTGUN_SUPPRESSOR, "COMPONENT_AT_AR_SUPP_02" },
                { ItemCatalogIds.COMPONENT_SHOTGUN_GRIP, "COMPONENT_AT_AR_AFGRIP" }
            }
        },
        {
            ItemCatalogIds.WEAPON_HEAVY_SHOTGUN,
            new Dictionary<ItemCatalogIds, string>
            {
                { ItemCatalogIds.COMPONENT_SHOTGUN_EXTENDED_CLIP, "COMPONENT_HEAVYSHOTGUN_CLIP_02" },
                { ItemCatalogIds.COMPONENT_SHOTGUN_FLASHLIGHT, "COMPONENT_AT_AR_FLSH" },
                { ItemCatalogIds.COMPONENT_SHOTGUN_SUPPRESSOR, "COMPONENT_AT_AR_SUPP_02" },
                { ItemCatalogIds.COMPONENT_SHOTGUN_GRIP, "COMPONENT_AT_AR_AFGRIP" }
            }
        },
        {
            ItemCatalogIds.WEAPON_ASSAULT_RIFLE,
            new Dictionary<ItemCatalogIds, string>
            {
                { ItemCatalogIds.COMPONENT_RIFLE_EXTENDED_CLIP, "COMPONENT_ASSAULTRIFLE_CLIP_02" },
                { ItemCatalogIds.COMPONENT_RIFLE_FLASHLIGHT, "COMPONENT_AT_AR_FLSH" },
                { ItemCatalogIds.COMPONENT_RIFLE_SUPPRESSOR, "COMPONENT_AT_AR_SUPP_02" },
                { ItemCatalogIds.COMPONENT_RIFLE_GRIP, "COMPONENT_AT_AR_AFGRIP" },
                { ItemCatalogIds.COMPONENT_RIFLE_SCOPE1, "COMPONENT_AT_SCOPE_MACRO" },
                { ItemCatalogIds.COMPONENT_ASSAULTRIFLE_VARMOD_LUXE, "COMPONENT_ASSAULTRIFLE_VARMOD_LUXE" }
            }
        },
        {
            ItemCatalogIds.WEAPON_CARBINE_RIFLE,
            new Dictionary<ItemCatalogIds, string>
            {
                { ItemCatalogIds.COMPONENT_RIFLE_EXTENDED_CLIP, "COMPONENT_CARBINERIFLE_CLIP_02" },
                { ItemCatalogIds.COMPONENT_RIFLE_FLASHLIGHT, "COMPONENT_AT_AR_FLSH" },
                { ItemCatalogIds.COMPONENT_RIFLE_SUPPRESSOR, "COMPONENT_AT_AR_SUPP" },
                { ItemCatalogIds.COMPONENT_RIFLE_SCOPE1, "COMPONENT_AT_SCOPE_MEDIUM" },
                { ItemCatalogIds.COMPONENT_RIFLE_GRIP, "COMPONENT_AT_AR_AFGRIP" },
                { ItemCatalogIds.COMPONENT_CARBINERIFLE_VARMOD_LUXE, "COMPONENT_CARBINERIFLE_VARMOD_LUXE" }
            }
        },
        {
            ItemCatalogIds.WEAPON_ADVANCED_RIFLE,
            new Dictionary<ItemCatalogIds, string>
            {
                { ItemCatalogIds.COMPONENT_RIFLE_EXTENDED_CLIP, "COMPONENT_ADVANCEDRIFLE_CLIP_02" },
                { ItemCatalogIds.COMPONENT_RIFLE_FLASHLIGHT, "COMPONENT_AT_AR_FLSH" },
                { ItemCatalogIds.COMPONENT_RIFLE_SUPPRESSOR, "COMPONENT_AT_AR_SUPP" },
                { ItemCatalogIds.COMPONENT_RIFLE_SCOPE1, "COMPONENT_AT_SCOPE_SMALL" },
                { ItemCatalogIds.COMPONENT_ADVANCEDRIFLE_VARMOD_LUXE, "COMPONENT_ADVANCEDRIFLE_VARMOD_LUXE" }
            }
        },
        {
            ItemCatalogIds.WEAPON_SPECIAL_CARBINE,
            new Dictionary<ItemCatalogIds, string>
            {
                { ItemCatalogIds.COMPONENT_RIFLE_EXTENDED_CLIP, "COMPONENT_SPECIALCARBINE_CLIP_02" },
                { ItemCatalogIds.COMPONENT_RIFLE_FLASHLIGHT, "COMPONENT_AT_AR_FLSH" },
                { ItemCatalogIds.COMPONENT_RIFLE_SUPPRESSOR, "COMPONENT_AT_AR_SUPP_02" },
                { ItemCatalogIds.COMPONENT_RIFLE_GRIP, "COMPONENT_AT_AR_AFGRIP" },
                { ItemCatalogIds.COMPONENT_RIFLE_SCOPE1, "COMPONENT_AT_SCOPE_MEDIUM" },
                {
                    ItemCatalogIds.COMPONENT_SPECIALCARBINE_VARMOD_LOWRIDER,
                    "COMPONENT_SPECIALCARBINE_VARMOD_LOWRIDER"
                }
            }
        },
        {
            ItemCatalogIds.WEAPON_BULLPUP_RIFLE,
            new Dictionary<ItemCatalogIds, string>
            {
                { ItemCatalogIds.COMPONENT_RIFLE_EXTENDED_CLIP, "COMPONENT_BULLPUPRIFLE_CLIP_02" },
                { ItemCatalogIds.COMPONENT_RIFLE_FLASHLIGHT, "COMPONENT_AT_AR_FLSH" },
                { ItemCatalogIds.COMPONENT_RIFLE_SCOPE1, "COMPONENT_AT_SCOPE_SMALL" },
                { ItemCatalogIds.COMPONENT_RIFLE_SUPPRESSOR, "COMPONENT_AT_AR_SUPP" },
                { ItemCatalogIds.COMPONENT_RIFLE_GRIP, "COMPONENT_AT_AR_AFGRIP" },
                { ItemCatalogIds.COMPONENT_BULLPUPRIFLE_VARMOD_LOW, "COMPONENT_BULLPUPRIFLE_VARMOD_LOW" }
            }
        },
        {
            ItemCatalogIds.WEAPON_BULLPUP_RIFLE_MK_II,
            new Dictionary<ItemCatalogIds, string>
            {
                { ItemCatalogIds.COMPONENT_RIFLE_EXTENDED_CLIP, "COMPONENT_BULLPUPRIFLE_MK2_CLIP_02" },
                { ItemCatalogIds.COMPONENT_RIFLE_FLASHLIGHT, "COMPONENT_AT_AR_FLSH" },
                { ItemCatalogIds.COMPONENT_RIFLE_SCOPE1, "COMPONENT_AT_SIGHTS" },
                { ItemCatalogIds.COMPONENT_RIFLE_SUPPRESSOR, "COMPONENT_AT_AR_SUPP" },
                { ItemCatalogIds.COMPONENT_RIFLE_GRIP, "COMPONENT_AT_AR_AFGRIP_02" }
            }
        },
        {
            ItemCatalogIds.WEAPON_SPECIAL_CARBINE_MK_II,
            new Dictionary<ItemCatalogIds, string>
            {
                { ItemCatalogIds.COMPONENT_RIFLE_EXTENDED_CLIP, "COMPONENT_SPECIALCARBINE_MK2_CLIP_02" },
                { ItemCatalogIds.COMPONENT_RIFLE_FLASHLIGHT, "COMPONENT_AT_AR_FLSH" },
                { ItemCatalogIds.COMPONENT_RIFLE_SCOPE1, "COMPONENT_AT_SIGHTS" },
                { ItemCatalogIds.COMPONENT_RIFLE_SUPPRESSOR, "COMPONENT_AT_AR_SUPP_02" },
                { ItemCatalogIds.COMPONENT_RIFLE_GRIP, "COMPONENT_AT_AR_AFGRIP_02" }
            }
        },
        {
            ItemCatalogIds.WEAPON_ASSAULT_RIFLE_MK_II,
            new Dictionary<ItemCatalogIds, string>
            {
                { ItemCatalogIds.COMPONENT_RIFLE_EXTENDED_CLIP, "COMPONENT_ASSAULTRIFLE_MK2_CLIP_02" },
                { ItemCatalogIds.COMPONENT_RIFLE_FLASHLIGHT, "COMPONENT_AT_AR_FLSH" },
                { ItemCatalogIds.COMPONENT_RIFLE_SCOPE1, "COMPONENT_AT_SIGHTS" },
                { ItemCatalogIds.COMPONENT_RIFLE_SUPPRESSOR, "COMPONENT_AT_AR_SUPP_02" },
                { ItemCatalogIds.COMPONENT_RIFLE_GRIP, "COMPONENT_AT_AR_AFGRIP_02" }
            }
        },
        {
            ItemCatalogIds.WEAPON_CARBINE_RIFLE_MK_II,
            new Dictionary<ItemCatalogIds, string>
            {
                { ItemCatalogIds.COMPONENT_RIFLE_EXTENDED_CLIP, "COMPONENT_CARBINERIFLE_MK2_CLIP_02" },
                { ItemCatalogIds.COMPONENT_RIFLE_FLASHLIGHT, "COMPONENT_AT_AR_FLSH" },
                { ItemCatalogIds.COMPONENT_RIFLE_SCOPE1, "COMPONENT_AT_SIGHTS" },
                { ItemCatalogIds.COMPONENT_RIFLE_SUPPRESSOR, "COMPONENT_AT_AR_SUPP" },
                { ItemCatalogIds.COMPONENT_RIFLE_GRIP, "COMPONENT_AT_AR_AFGRIP_02" }
            }
        },
        {
            ItemCatalogIds.WEAPON_COMPACT_RIFLE,
            new Dictionary<ItemCatalogIds, string>
            {
                { ItemCatalogIds.COMPONENT_RIFLE_EXTENDED_CLIP, "COMPONENT_COMPACTRIFLE_CLIP_02" }
            }
        },
        {
            ItemCatalogIds.WEAPON_MG,
            new Dictionary<ItemCatalogIds, string>
            {
                { ItemCatalogIds.COMPONENT_MG_EXTENDED_CLIP, "COMPONENT_MG_CLIP_02" },
                { ItemCatalogIds.COMPONENT_MG_SCOPE1, "COMPONENT_AT_SCOPE_SMALL_02" },
                { ItemCatalogIds.COMPONENT_MG_VARMOD_LOWRIDER, "COMPONENT_MG_VARMOD_LOWRIDER" }
            }
        },
        {
            ItemCatalogIds.WEAPON_COMBAT_MG,
            new Dictionary<ItemCatalogIds, string>
            {
                { ItemCatalogIds.COMPONENT_MG_EXTENDED_CLIP, "COMPONENT_COMBATMG_CLIP_02" },
                { ItemCatalogIds.COMPONENT_MG_SCOPE1, "COMPONENT_AT_SCOPE_MEDIUM" },
                { ItemCatalogIds.COMPONENT_MG_GRIP, "COMPONENT_AT_AR_AFGRIP" },
                { ItemCatalogIds.COMPONENT_COMBATMG_VARMOD_LOWRIDER, "COMPONENT_COMBATMG_VARMOD_LOWRIDER" }
            }
        },
        {
            ItemCatalogIds.WEAPON_COMBAT_MG_MK_II,
            new Dictionary<ItemCatalogIds, string>
            {
                { ItemCatalogIds.COMPONENT_MG_EXTENDED_CLIP, "COMPONENT_COMBATMG_MK2_CLIP_02" },
                { ItemCatalogIds.COMPONENT_MG_SCOPE1, "COMPONENT_AT_SIGHTS" },
                { ItemCatalogIds.COMPONENT_MG_GRIP, "COMPONENT_AT_AR_AFGRIP_02" }
            }
        },
        {
            ItemCatalogIds.WEAPON_GUSENBERG_SWEEPER,
            new Dictionary<ItemCatalogIds, string>
            {
                { ItemCatalogIds.COMPONENT_RIFLE_EXTENDED_CLIP, "COMPONENT_GUSENBERG_CLIP_02" }
            }
        },
        {
            ItemCatalogIds.WEAPON_SNIPER_RIFLE,
            new Dictionary<ItemCatalogIds, string>
            {
                { ItemCatalogIds.COMPONENT_SNIPER_SUPPRESSOR, "COMPONENT_AT_AR_SUPP_02" },
                { ItemCatalogIds.COMPONENT_SNIPER_SCOPE1, "COMPONENT_AT_SCOPE_LARGE" },
                { ItemCatalogIds.COMPONENT_SNIPERRIFLE_VARMOD_LUXE, "COMPONENT_SNIPERRIFLE_VARMOD_LUXE" }
            }
        },
        {
            ItemCatalogIds.WEAPON_HEAVY_SNIPER,
            new Dictionary<ItemCatalogIds, string>
            {
                { ItemCatalogIds.COMPONENT_SNIPER_SCOPE1, "COMPONENT_AT_SCOPE_LARGE" },
                { ItemCatalogIds.COMPONENT_SNIPER_SCOPE2, "COMPONENT_AT_SCOPE_MAX" }
            }
        },
        {
            ItemCatalogIds.WEAPON_MARKSMAN_RIFLE_MK_II,
            new Dictionary<ItemCatalogIds, string>
            {
                { ItemCatalogIds.COMPONENT_SNIPER_EXTENDED_CLIP, "COMPONENT_MARKSMANRIFLE_MK2_CLIP_02" },
                { ItemCatalogIds.COMPONENT_SNIPER_SUPPRESSOR, "COMPONENT_AT_AR_SUPP" },
                { ItemCatalogIds.COMPONENT_SNIPER_FLASHLIGHT, "COMPONENT_AT_AR_FLSH" },
                { ItemCatalogIds.COMPONENT_SNIPER_SCOPE1, "COMPONENT_AT_SIGHTS" },
                { ItemCatalogIds.COMPONENT_SNIPER_SCOPE2, "COMPONENT_AT_SCOPE_MEDIUM_MK2" },
                { ItemCatalogIds.COMPONENT_SNIPER_GRIP, "COMPONENT_AT_AR_AFGRIP_02" }
            }
        },
        {
            ItemCatalogIds.WEAPON_HEAVY_SNIPER_MK_II,
            new Dictionary<ItemCatalogIds, string>
            {
                { ItemCatalogIds.COMPONENT_SNIPER_EXTENDED_CLIP, "COMPONENT_HEAVYSNIPER_MK2_CLIP_02" },
                { ItemCatalogIds.COMPONENT_SNIPER_SUPPRESSOR, "COMPONENT_AT_SR_SUPP_03" },
                { ItemCatalogIds.COMPONENT_SNIPER_FLASHLIGHT, "COMPONENT_AT_AR_FLSH" },
                { ItemCatalogIds.COMPONENT_SNIPER_SCOPE1, "COMPONENT_AT_SCOPE_LARGE_MK2" },
                { ItemCatalogIds.COMPONENT_SNIPER_SCOPE2, "COMPONENT_AT_SCOPE_MAX" }
            }
        },
        {
            ItemCatalogIds.WEAPON_MARKSMAN_RIFLE,
            new Dictionary<ItemCatalogIds, string>
            {
                { ItemCatalogIds.COMPONENT_SNIPER_EXTENDED_CLIP, "COMPONENT_MARKSMANRIFLE_CLIP_02" },
                { ItemCatalogIds.COMPONENT_SNIPER_SUPPRESSOR, "COMPONENT_AT_AR_SUPP" },
                { ItemCatalogIds.COMPONENT_SNIPER_FLASHLIGHT, "COMPONENT_AT_AR_FLSH" },
                { ItemCatalogIds.COMPONENT_SNIPER_SCOPE1, "COMPONENT_AT_SCOPE_LARGE_FIXED_ZOOM" },
                { ItemCatalogIds.COMPONENT_SNIPER_GRIP, "COMPONENT_AT_AR_AFGRIP" },
                { ItemCatalogIds.COMPONENT_MARKSMANRIFLE_VARMOD_LUXE, "COMPONENT_MARKSMANRIFLE_VARMOD_LUXE" }
            }
        }
    };

    private readonly InventoryModule _inventoryModule;
    private readonly ItemService _itemService;

    private readonly ILogger<AttachmentModule> _logger;

    public AttachmentModule(ILogger<AttachmentModule> logger, ItemService itemService, InventoryModule inventoryModule)
    {
        _logger = logger;
        _itemService = itemService;
        _inventoryModule = inventoryModule;
    }

    public static bool IsItemWeaponComponent(ItemCatalogIds itemCatalogId)
    {
        switch (itemCatalogId)
        {
            case ItemCatalogIds.COMPONENT_KNUCKLE_VARMOD_PIMP:
            case ItemCatalogIds.COMPONENT_KNUCKLE_VARMOD_BALLAS:
            case ItemCatalogIds.COMPONENT_KNUCKLE_VARMOD_DOLLAR:
            case ItemCatalogIds.COMPONENT_KNUCKLE_VARMOD_DIAMOND:
            case ItemCatalogIds.COMPONENT_KNUCKLE_VARMOD_HATE:
            case ItemCatalogIds.COMPONENT_KNUCKLE_VARMOD_LOVE:
            case ItemCatalogIds.COMPONENT_KNUCKLE_VARMOD_PLAYER:
            case ItemCatalogIds.COMPONENT_KNUCKLE_VARMOD_KING:
            case ItemCatalogIds.COMPONENT_KNUCKLE_VARMOD_VAGOS:
            case ItemCatalogIds.COMPONENT_SWITCHBLADE_VARMOD_VAR1:
            case ItemCatalogIds.COMPONENT_SWITCHBLADE_VARMOD_VAR2:
            case ItemCatalogIds.COMPONENT_PISTOL_VARMOD_LUXE:
            case ItemCatalogIds.COMPONENT_COMBATPISTOL_VARMOD_LOWRIDER:
            case ItemCatalogIds.COMPONENT_APPISTOL_VARMOD_LUXE:
            case ItemCatalogIds.COMPONENT_PISTOL50_VARMOD_LUXE:
            case ItemCatalogIds.COMPONENT_REVOLVER_VARMOD_BOSS:
            case ItemCatalogIds.COMPONENT_REVOLVER_VARMOD_GOON:
            case ItemCatalogIds.COMPONENT_SNSPISTOL_VARMOD_LOWRIDER:
            case ItemCatalogIds.COMPONENT_HEAVYPISTOL_VARMOD_LUXE:
            case ItemCatalogIds.COMPONENT_MICROSMG_VARMOD_LUXE:
            case ItemCatalogIds.COMPONENT_SMG_VARMOD_LUXE:
            case ItemCatalogIds.COMPONENT_ASSAULTSMG_VARMOD_LOWRIDER:
            case ItemCatalogIds.COMPONENT_PUMPSHOTGUN_VARMOD_LOWRIDER:
            case ItemCatalogIds.COMPONENT_SAWNOFFSHOTGUN_VARMOD_LUXE:
            case ItemCatalogIds.COMPONENT_ASSAULTRIFLE_VARMOD_LUXE:
            case ItemCatalogIds.COMPONENT_CARBINERIFLE_VARMOD_LUXE:
            case ItemCatalogIds.COMPONENT_ADVANCEDRIFLE_VARMOD_LUXE:
            case ItemCatalogIds.COMPONENT_SPECIALCARBINE_VARMOD_LOWRIDER:
            case ItemCatalogIds.COMPONENT_BULLPUPRIFLE_VARMOD_LOW:
            case ItemCatalogIds.COMPONENT_MG_VARMOD_LOWRIDER:
            case ItemCatalogIds.COMPONENT_COMBATMG_VARMOD_LOWRIDER:
            case ItemCatalogIds.COMPONENT_SNIPERRIFLE_VARMOD_LUXE:
            case ItemCatalogIds.COMPONENT_MARKSMANRIFLE_VARMOD_LUXE:
            case ItemCatalogIds.COMPONENT_PISTOL_EXTENDED_CLIP:
            case ItemCatalogIds.COMPONENT_PISTOL_FLASHLIGHT:
            case ItemCatalogIds.COMPONENT_PISTOL_SUPPRESSOR:
            case ItemCatalogIds.COMPONENT_SMG_EXTENDED_CLIP:
            case ItemCatalogIds.COMPONENT_SMG_FLASHLIGHT:
            case ItemCatalogIds.COMPONENT_SMG_SUPPRESSOR:
            case ItemCatalogIds.COMPONENT_SMG_SCOPE1:
            case ItemCatalogIds.COMPONENT_SHOTGUN_EXTENDED_CLIP:
            case ItemCatalogIds.COMPONENT_SHOTGUN_FLASHLIGHT:
            case ItemCatalogIds.COMPONENT_SHOTGUN_SUPPRESSOR:
            case ItemCatalogIds.COMPONENT_RIFLE_EXTENDED_CLIP:
            case ItemCatalogIds.COMPONENT_RIFLE_FLASHLIGHT:
            case ItemCatalogIds.COMPONENT_RIFLE_SUPPRESSOR:
            case ItemCatalogIds.COMPONENT_RIFLE_SCOPE1:
            case ItemCatalogIds.COMPONENT_RIFLE_GRIP:
            case ItemCatalogIds.COMPONENT_MG_EXTENDED_CLIP:
            case ItemCatalogIds.COMPONENT_MG_SCOPE1:
            case ItemCatalogIds.COMPONENT_MG_GRIP:
            case ItemCatalogIds.COMPONENT_SNIPER_EXTENDED_CLIP:
            case ItemCatalogIds.COMPONENT_SNIPER_SUPPRESSOR:
            case ItemCatalogIds.COMPONENT_SNIPER_FLASHLIGHT:
            case ItemCatalogIds.COMPONENT_SNIPER_SCOPE1:
            case ItemCatalogIds.COMPONENT_SNIPER_SCOPE2:
            case ItemCatalogIds.COMPONENT_SNIPER_GRIP:
                return true;
            default:
                return false;
        }
    }

    public static async Task AddWeaponComponent(ServerPlayer player, ItemWeaponModel itemWeaponModel,
        string componentHash)
    {
        var weaponModel = WeaponModule.GetModelFromId(itemWeaponModel.CatalogItemModelId);

        await player.AddWeaponComponentAsync((uint)weaponModel, Alt.Hash(componentHash));
    }

    public async Task AddToWeapon(ServerPlayer player, ItemWeaponModel itemWeaponModel,
        ItemWeaponAttachmentModel weaponAttachmentModel)
    {
        if (weaponAttachmentModel.ItemWeaponId.HasValue)
        {
            return;
        }

        if (!ComponentTable.TryGetValue(itemWeaponModel.CatalogItemModelId, out var weaponComponentTable))
        {
            player.SendNotification("Diese Erweiterung passt nicht auf die Waffe.", NotificationType.ERROR);
            return;
        }

        if (!weaponComponentTable.TryGetValue(weaponAttachmentModel.CatalogItemModelId, out var weaponComponentHash))
        {
            player.SendNotification("Diese Erweiterung passt nicht auf die Waffe.", NotificationType.ERROR);
            return;
        }

        var weaponModel = WeaponModule.GetModelFromId(itemWeaponModel.CatalogItemModelId);

        if (itemWeaponModel.ComponentHashes.Contains(weaponComponentHash))
        {
            player.SendNotification("Du kannst nicht mehrere Erweiterungen dieser Sorte montieren.",
                NotificationType.ERROR);
            return;
        }

        itemWeaponModel.ComponentHashes.Add(weaponComponentHash);
        weaponAttachmentModel.ItemWeaponId = itemWeaponModel.Id;
        weaponAttachmentModel.Slot = null;

        await player.AddWeaponComponentAsync((uint)weaponModel, Alt.Hash(weaponComponentHash));

        await _itemService.Update(itemWeaponModel);
        await _itemService.Update(weaponAttachmentModel);
    }

    public async Task RemoveFromWeapon(ServerPlayer player, ItemWeaponModel itemWeaponModel,
        ItemWeaponAttachmentModel weaponAttachmentModel)
    {
        if (!weaponAttachmentModel.ItemWeaponId.HasValue)
        {
            return;
        }

        var freeSlot = await _inventoryModule.GetFreeNextSlot(player.CharacterModel.InventoryModel.Id,
            weaponAttachmentModel.CatalogItemModel.Weight);
        if (!freeSlot.HasValue)
        {
            player.SendNotification("Dein Charakter hat nicht genug Platz im Inventar für die Erweiterungen.",
                NotificationType.ERROR);
            return;
        }

        weaponAttachmentModel.ItemWeaponId = null;
        weaponAttachmentModel.Slot = freeSlot.Value;

        var weaponComponentHash =
            ComponentTable[itemWeaponModel.CatalogItemModelId][weaponAttachmentModel.CatalogItemModelId];

        var weaponModel = WeaponModule.GetModelFromId(itemWeaponModel.CatalogItemModelId);

        if (itemWeaponModel.ComponentHashes.Contains(weaponComponentHash))
        {
            itemWeaponModel.ComponentHashes.Remove(weaponComponentHash);
        }

        await player.RemoveWeaponComponentAsync((uint)weaponModel, Alt.Hash(weaponComponentHash));

        await _itemService.Update(itemWeaponModel);
        await _itemService.Update(weaponAttachmentModel);
    }
}