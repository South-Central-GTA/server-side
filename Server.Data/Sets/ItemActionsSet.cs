﻿using System.Collections.Generic;
using Server.Data.Models;
using Server.Database.Enums;

namespace Server.Data.Sets;

public static class ItemActionsSet
{
    private static readonly Dictionary<int, List<ActionData>> Actions = new()
    {
        { (int)ItemCatalogIds.DOLLAR, new List<ActionData>() },
        { (int)ItemCatalogIds.PHONE, new List<ActionData> { new("Als aktives Handy setzen", "phone:setasactive") } },
        { (int)ItemCatalogIds.KEY, new List<ActionData> { new("Eine Kopie anfertigen", "key:createcopy") } },
        { (int)ItemCatalogIds.CLOTHING_HAT, new List<ActionData>() },
        { (int)ItemCatalogIds.CLOTHING_GLASSES, new List<ActionData>() },
        { (int)ItemCatalogIds.CLOTHING_EARS, new List<ActionData>() },
        { (int)ItemCatalogIds.CLOTHING_MASK, new List<ActionData>() },
        { (int)ItemCatalogIds.CLOTHING_TOP, new List<ActionData>() },
        { (int)ItemCatalogIds.CLOTHING_WATCH, new List<ActionData>() },
        { (int)ItemCatalogIds.CLOTHING_BRACELET, new List<ActionData>() },
        { (int)ItemCatalogIds.CLOTHING_UNDERSHIRT, new List<ActionData>() },
        { (int)ItemCatalogIds.CLOTHING_ACCESSORIES, new List<ActionData>() },
        { (int)ItemCatalogIds.CLOTHING_PANTS, new List<ActionData>() },
        { (int)ItemCatalogIds.CLOTHING_BACKPACK, new List<ActionData>() },
        { (int)ItemCatalogIds.CLOTHING_BODY_ARMOR, new List<ActionData>() },
        { (int)ItemCatalogIds.CLOTHING_SHOES, new List<ActionData>() },
        {
            (int)ItemCatalogIds.NON_ALC_DRINKS, new List<ActionData>
            {
                // new ActionData("Zerbrechen", "glassbottle:craft")
            }
        },
        {
            (int)ItemCatalogIds.ALC_DRINKS, new List<ActionData>
            {
                // new ActionData("Zerbrechen", "glassbottle:craft")
            }
        },
        { (int)ItemCatalogIds.FAST_FOOD, new List<ActionData>() },
        { (int)ItemCatalogIds.HEALTHY_FOOD, new List<ActionData>() },
        { (int)ItemCatalogIds.BREAD, new List<ActionData>() },
        { (int)ItemCatalogIds.SANDWICH, new List<ActionData>() },
        { (int)ItemCatalogIds.SOUP, new List<ActionData>() },
        { (int)ItemCatalogIds.MEAT, new List<ActionData>() },
        { (int)ItemCatalogIds.SWEETS, new List<ActionData>() },
        { (int)ItemCatalogIds.CANDY, new List<ActionData>() },
        { (int)ItemCatalogIds.GROUP_KEY, new List<ActionData>() },
        { (int)ItemCatalogIds.HANDCUFF_KEY, new List<ActionData>() },
        { (int)ItemCatalogIds.RADIO, new List<ActionData> { new("Frequenz einstellen", "radio:openrequest") } },
        { (int)ItemCatalogIds.MEGAPHONE, new List<ActionData>() },
        { (int)ItemCatalogIds.TRAFFIC_CONE, new List<ActionData>() },
        { (int)ItemCatalogIds.HANDCUFF, new List<ActionData>() },
        { (int)ItemCatalogIds.LICENSES, new List<ActionData> { new("Anschauen", "licenses:requestopen") } },
        { (int)ItemCatalogIds.REPAIR_KIT, new List<ActionData> { new("Fahrzeug reparieren", "repairkit:requestuse") } },
        { (int)ItemCatalogIds.WEAPON_ANTIQUE_CAVALRY_DAGGER, new List<ActionData>() },
        { (int)ItemCatalogIds.WEAPON_BASEBALL_BAT, new List<ActionData>() },
        { (int)ItemCatalogIds.WEAPON_BROKEN_BOTTLE, new List<ActionData>() },
        { (int)ItemCatalogIds.WEAPON_CROWBAR, new List<ActionData>() },
        { (int)ItemCatalogIds.WEAPON_FLASHLIGHT, new List<ActionData>() },
        { (int)ItemCatalogIds.WEAPON_GOLF_CLUB, new List<ActionData>() },
        { (int)ItemCatalogIds.WEAPON_HAMMER, new List<ActionData>() },
        { (int)ItemCatalogIds.WEAPON_HATCHET, new List<ActionData>() },
        { (int)ItemCatalogIds.WEAPON_BRASS_KNUCKLES, new List<ActionData>() },
        { (int)ItemCatalogIds.WEAPON_KNIFE, new List<ActionData>() },
        { (int)ItemCatalogIds.WEAPON_MACHETE, new List<ActionData>() },
        { (int)ItemCatalogIds.WEAPON_SWITCHBLADE, new List<ActionData>() },
        { (int)ItemCatalogIds.WEAPON_NIGHTSTICK, new List<ActionData>() },
        { (int)ItemCatalogIds.WEAPON_PIPE_WRENCH, new List<ActionData>() },
        { (int)ItemCatalogIds.WEAPON_BATTLE_AXE, new List<ActionData>() },
        { (int)ItemCatalogIds.WEAPON_POOL_CUE, new List<ActionData>() },
        { (int)ItemCatalogIds.WEAPON_STONE_HATCHET, new List<ActionData>() },
        {
            (int)ItemCatalogIds.WEAPON_PISTOL,
            new List<ActionData>
            {
                new("Seriennummer anschauen", "serialnumber:show"),
                new("Seriennummer wegkratzen", "serialnumber:requestremove")
            }
        },
        {
            (int)ItemCatalogIds.WEAPON_PISTOL_MK_II,
            new List<ActionData>
            {
                new("Seriennummer anschauen", "serialnumber:show"),
                new("Seriennummer wegkratzen", "serialnumber:requestremove")
            }
        },
        {
            (int)ItemCatalogIds.WEAPON_COMBAT_PISTOL,
            new List<ActionData>
            {
                new("Seriennummer anschauen", "serialnumber:show"),
                new("Seriennummer wegkratzen", "serialnumber:requestremove")
            }
        },
        {
            (int)ItemCatalogIds.WEAPON_AP_PISTOL,
            new List<ActionData>
            {
                new("Seriennummer anschauen", "serialnumber:show"),
                new("Seriennummer wegkratzen", "serialnumber:requestremove")
            }
        },
        {
            (int)ItemCatalogIds.WEAPON_STUN_GUN,
            new List<ActionData>
            {
                new("Seriennummer anschauen", "serialnumber:show"),
                new("Seriennummer wegkratzen", "serialnumber:requestremove")
            }
        },
        {
            (int)ItemCatalogIds.WEAPON_PISTOL50,
            new List<ActionData>
            {
                new("Seriennummer anschauen", "serialnumber:show"),
                new("Seriennummer wegkratzen", "serialnumber:requestremove")
            }
        },
        {
            (int)ItemCatalogIds.WEAPON_SNS_PISTOL,
            new List<ActionData>
            {
                new("Seriennummer anschauen", "serialnumber:show"),
                new("Seriennummer wegkratzen", "serialnumber:requestremove")
            }
        },
        {
            (int)ItemCatalogIds.WEAPON_SNS_PISTOL_MK_II,
            new List<ActionData>
            {
                new("Seriennummer anschauen", "serialnumber:show"),
                new("Seriennummer wegkratzen", "serialnumber:requestremove")
            }
        },
        {
            (int)ItemCatalogIds.WEAPON_HEAVY_PISTOL,
            new List<ActionData>
            {
                new("Seriennummer anschauen", "serialnumber:show"),
                new("Seriennummer wegkratzen", "serialnumber:requestremove")
            }
        },
        {
            (int)ItemCatalogIds.WEAPON_VINTAGE_PISTOL,
            new List<ActionData>
            {
                new("Seriennummer anschauen", "serialnumber:show"),
                new("Seriennummer wegkratzen", "serialnumber:requestremove")
            }
        },
        {
            (int)ItemCatalogIds.WEAPON_FLARE_GUN,
            new List<ActionData>
            {
                new("Seriennummer anschauen", "serialnumber:show"),
                new("Seriennummer wegkratzen", "serialnumber:requestremove")
            }
        },
        {
            (int)ItemCatalogIds.WEAPON_MARKSMAN_PISTOL,
            new List<ActionData>
            {
                new("Seriennummer anschauen", "serialnumber:show"),
                new("Seriennummer wegkratzen", "serialnumber:requestremove")
            }
        },
        {
            (int)ItemCatalogIds.WEAPON_HEAVY_REVOLVER,
            new List<ActionData>
            {
                new("Seriennummer anschauen", "serialnumber:show"),
                new("Seriennummer wegkratzen", "serialnumber:requestremove")
            }
        },
        {
            (int)ItemCatalogIds.WEAPON_HEAVY_REVOLVER_MK_II,
            new List<ActionData>
            {
                new("Seriennummer anschauen", "serialnumber:show"),
                new("Seriennummer wegkratzen", "serialnumber:requestremove")
            }
        },
        {
            (int)ItemCatalogIds.WEAPON_DOUBLE_ACTION_REVOLVER,
            new List<ActionData>
            {
                new("Seriennummer anschauen", "serialnumber:show"),
                new("Seriennummer wegkratzen", "serialnumber:requestremove")
            }
        },
        {
            (int)ItemCatalogIds.WEAPON_MICRO_SMG,
            new List<ActionData>
            {
                new("Seriennummer anschauen", "serialnumber:show"),
                new("Seriennummer wegkratzen", "serialnumber:requestremove")
            }
        },
        {
            (int)ItemCatalogIds.WEAPON_SMG,
            new List<ActionData>
            {
                new("Seriennummer anschauen", "serialnumber:show"),
                new("Seriennummer wegkratzen", "serialnumber:requestremove")
            }
        },
        {
            (int)ItemCatalogIds.WEAPON_SMG_MK_II,
            new List<ActionData>
            {
                new("Seriennummer anschauen", "serialnumber:show"),
                new("Seriennummer wegkratzen", "serialnumber:requestremove")
            }
        },
        {
            (int)ItemCatalogIds.WEAPON_ASSAULT_SMG,
            new List<ActionData>
            {
                new("Seriennummer anschauen", "serialnumber:show"),
                new("Seriennummer wegkratzen", "serialnumber:requestremove")
            }
        },
        {
            (int)ItemCatalogIds.WEAPON_COMBAT_PDW,
            new List<ActionData>
            {
                new("Seriennummer anschauen", "serialnumber:show"),
                new("Seriennummer wegkratzen", "serialnumber:requestremove")
            }
        },
        {
            (int)ItemCatalogIds.WEAPON_MACHINE_PISTOL,
            new List<ActionData>
            {
                new("Seriennummer anschauen", "serialnumber:show"),
                new("Seriennummer wegkratzen", "serialnumber:requestremove")
            }
        },
        {
            (int)ItemCatalogIds.WEAPON_MINI_SMG,
            new List<ActionData>
            {
                new("Seriennummer anschauen", "serialnumber:show"),
                new("Seriennummer wegkratzen", "serialnumber:requestremove")
            }
        },
        {
            (int)ItemCatalogIds.WEAPON_PUMP_SHOTGUN,
            new List<ActionData>
            {
                new("Seriennummer anschauen", "serialnumber:show"),
                new("Seriennummer wegkratzen", "serialnumber:requestremove")
            }
        },
        {
            (int)ItemCatalogIds.WEAPON_PUMP_SHOTGUN_MK_II,
            new List<ActionData>
            {
                new("Seriennummer anschauen", "serialnumber:show"),
                new("Seriennummer wegkratzen", "serialnumber:requestremove")
            }
        },
        {
            (int)ItemCatalogIds.WEAPON_SAWED_OFF_SHOTGUN,
            new List<ActionData>
            {
                new("Seriennummer anschauen", "serialnumber:show"),
                new("Seriennummer wegkratzen", "serialnumber:requestremove")
            }
        },
        {
            (int)ItemCatalogIds.WEAPON_ASSAULT_SHOTGUN,
            new List<ActionData>
            {
                new("Seriennummer anschauen", "serialnumber:show"),
                new("Seriennummer wegkratzen", "serialnumber:requestremove")
            }
        },
        {
            (int)ItemCatalogIds.WEAPON_BULLPUP_SHOTGUN,
            new List<ActionData>
            {
                new("Seriennummer anschauen", "serialnumber:show"),
                new("Seriennummer wegkratzen", "serialnumber:requestremove")
            }
        },
        {
            (int)ItemCatalogIds.WEAPON_MUSKET,
            new List<ActionData>
            {
                new("Seriennummer anschauen", "serialnumber:show"),
                new("Seriennummer wegkratzen", "serialnumber:requestremove")
            }
        },
        {
            (int)ItemCatalogIds.WEAPON_HEAVY_SHOTGUN,
            new List<ActionData>
            {
                new("Seriennummer anschauen", "serialnumber:show"),
                new("Seriennummer wegkratzen", "serialnumber:requestremove")
            }
        },
        {
            (int)ItemCatalogIds.WEAPON_DOUBLE_BARREL_SHOTGUN,
            new List<ActionData>
            {
                new("Seriennummer anschauen", "serialnumber:show"),
                new("Seriennummer wegkratzen", "serialnumber:requestremove")
            }
        },
        {
            (int)ItemCatalogIds.WEAPON_SWEEPER_SHOTGUN,
            new List<ActionData>
            {
                new("Seriennummer anschauen", "serialnumber:show"),
                new("Seriennummer wegkratzen", "serialnumber:requestremove")
            }
        },
        {
            (int)ItemCatalogIds.WEAPON_ASSAULT_RIFLE,
            new List<ActionData>
            {
                new("Seriennummer anschauen", "serialnumber:show"),
                new("Seriennummer wegkratzen", "serialnumber:requestremove")
            }
        },
        {
            (int)ItemCatalogIds.WEAPON_ASSAULT_RIFLE_MK_II,
            new List<ActionData>
            {
                new("Seriennummer anschauen", "serialnumber:show"),
                new("Seriennummer wegkratzen", "serialnumber:requestremove")
            }
        },
        {
            (int)ItemCatalogIds.WEAPON_CARBINE_RIFLE,
            new List<ActionData>
            {
                new("Seriennummer anschauen", "serialnumber:show"),
                new("Seriennummer wegkratzen", "serialnumber:requestremove")
            }
        },
        {
            (int)ItemCatalogIds.WEAPON_CARBINE_RIFLE_MK_II,
            new List<ActionData>
            {
                new("Seriennummer anschauen", "serialnumber:show"),
                new("Seriennummer wegkratzen", "serialnumber:requestremove")
            }
        },
        {
            (int)ItemCatalogIds.WEAPON_ADVANCED_RIFLE,
            new List<ActionData>
            {
                new("Seriennummer anschauen", "serialnumber:show"),
                new("Seriennummer wegkratzen", "serialnumber:requestremove")
            }
        },
        {
            (int)ItemCatalogIds.WEAPON_SPECIAL_CARBINE,
            new List<ActionData>
            {
                new("Seriennummer anschauen", "serialnumber:show"),
                new("Seriennummer wegkratzen", "serialnumber:requestremove")
            }
        },
        {
            (int)ItemCatalogIds.WEAPON_SPECIAL_CARBINE_MK_II,
            new List<ActionData>
            {
                new("Seriennummer anschauen", "serialnumber:show"),
                new("Seriennummer wegkratzen", "serialnumber:requestremove")
            }
        },
        {
            (int)ItemCatalogIds.WEAPON_BULLPUP_RIFLE,
            new List<ActionData>
            {
                new("Seriennummer anschauen", "serialnumber:show"),
                new("Seriennummer wegkratzen", "serialnumber:requestremove")
            }
        },
        {
            (int)ItemCatalogIds.WEAPON_BULLPUP_RIFLE_MK_II,
            new List<ActionData>
            {
                new("Seriennummer anschauen", "serialnumber:show"),
                new("Seriennummer wegkratzen", "serialnumber:requestremove")
            }
        },
        {
            (int)ItemCatalogIds.WEAPON_COMPACT_RIFLE,
            new List<ActionData>
            {
                new("Seriennummer anschauen", "serialnumber:show"),
                new("Seriennummer wegkratzen", "serialnumber:requestremove")
            }
        },
        {
            (int)ItemCatalogIds.WEAPON_MG,
            new List<ActionData>
            {
                new("Seriennummer anschauen", "serialnumber:show"),
                new("Seriennummer wegkratzen", "serialnumber:requestremove")
            }
        },
        {
            (int)ItemCatalogIds.WEAPON_COMBAT_MG,
            new List<ActionData>
            {
                new("Seriennummer anschauen", "serialnumber:show"),
                new("Seriennummer wegkratzen", "serialnumber:requestremove")
            }
        },
        {
            (int)ItemCatalogIds.WEAPON_COMBAT_MG_MK_II,
            new List<ActionData>
            {
                new("Seriennummer anschauen", "serialnumber:show"),
                new("Seriennummer wegkratzen", "serialnumber:requestremove")
            }
        },
        {
            (int)ItemCatalogIds.WEAPON_GUSENBERG_SWEEPER,
            new List<ActionData>
            {
                new("Seriennummer anschauen", "serialnumber:show"),
                new("Seriennummer wegkratzen", "serialnumber:requestremove")
            }
        },
        {
            (int)ItemCatalogIds.WEAPON_SNIPER_RIFLE,
            new List<ActionData>
            {
                new("Seriennummer anschauen", "serialnumber:show"),
                new("Seriennummer wegkratzen", "serialnumber:requestremove")
            }
        },
        {
            (int)ItemCatalogIds.WEAPON_HEAVY_SNIPER,
            new List<ActionData>
            {
                new("Seriennummer anschauen", "serialnumber:show"),
                new("Seriennummer wegkratzen", "serialnumber:requestremove")
            }
        },
        {
            (int)ItemCatalogIds.WEAPON_HEAVY_SNIPER_MK_II,
            new List<ActionData>
            {
                new("Seriennummer anschauen", "serialnumber:show"),
                new("Seriennummer wegkratzen", "serialnumber:requestremove")
            }
        },
        {
            (int)ItemCatalogIds.WEAPON_MARKSMAN_RIFLE,
            new List<ActionData>
            {
                new("Seriennummer anschauen", "serialnumber:show"),
                new("Seriennummer wegkratzen", "serialnumber:requestremove")
            }
        },
        {
            (int)ItemCatalogIds.WEAPON_MARKSMAN_RIFLE_MK_II,
            new List<ActionData>
            {
                new("Seriennummer anschauen", "serialnumber:show"),
                new("Seriennummer wegkratzen", "serialnumber:requestremove")
            }
        },
        { (int)ItemCatalogIds.WEAPON_GRENADE, new List<ActionData>() },
        { (int)ItemCatalogIds.WEAPON_BZ_GAS, new List<ActionData>() },
        { (int)ItemCatalogIds.WEAPON_MOLOTOV_COCKTAIL, new List<ActionData>() },
        { (int)ItemCatalogIds.WEAPON_SNOWBALL, new List<ActionData>() },
        { (int)ItemCatalogIds.WEAPON_BASEBALL, new List<ActionData>() },
        { (int)ItemCatalogIds.WEAPON_FLARE, new List<ActionData>() },
        { (int)ItemCatalogIds.WEAPON_JERRY_CAN, new List<ActionData>() },
        { (int)ItemCatalogIds.WEAPON_PARACHUTE, new List<ActionData>() },
        { (int)ItemCatalogIds.WEAPON_FIRE_EXTINGUISHER, new List<ActionData>() },
        {
            (int)ItemCatalogIds.WEAPON_MILITARY_RIFLE,
            new List<ActionData>
            {
                new("Seriennummer anschauen", "serialnumber:show"),
                new("Seriennummer wegkratzen", "serialnumber:requestremove")
            }
        },
        {
            (int)ItemCatalogIds.WEAPON_COMBAT_SHOTGUN,
            new List<ActionData>
            {
                new("Seriennummer anschauen", "serialnumber:show"),
                new("Seriennummer wegkratzen", "serialnumber:requestremove")
            }
        },
        { (int)ItemCatalogIds.AMMO_PISTOL, new List<ActionData>() },
        { (int)ItemCatalogIds.AMMO_MACHINE_GUN, new List<ActionData>() },
        { (int)ItemCatalogIds.AMMO_ASSAULT, new List<ActionData>() },
        { (int)ItemCatalogIds.AMMO_SNIPER, new List<ActionData>() },
        { (int)ItemCatalogIds.AMMO_SHOTGUN, new List<ActionData>() },
        { (int)ItemCatalogIds.AMMO_LIGHT_MACHINE_GUN, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_KNUCKLE_VARMOD_PIMP, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_KNUCKLE_VARMOD_BALLAS, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_KNUCKLE_VARMOD_DOLLAR, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_KNUCKLE_VARMOD_DIAMOND, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_KNUCKLE_VARMOD_HATE, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_KNUCKLE_VARMOD_LOVE, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_KNUCKLE_VARMOD_PLAYER, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_KNUCKLE_VARMOD_KING, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_KNUCKLE_VARMOD_VAGOS, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_SWITCHBLADE_VARMOD_VAR1, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_SWITCHBLADE_VARMOD_VAR2, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_PISTOL_VARMOD_LUXE, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_COMBATPISTOL_VARMOD_LOWRIDER, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_APPISTOL_VARMOD_LUXE, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_PISTOL50_VARMOD_LUXE, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_REVOLVER_VARMOD_BOSS, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_REVOLVER_VARMOD_GOON, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_SNSPISTOL_VARMOD_LOWRIDER, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_HEAVYPISTOL_VARMOD_LUXE, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_MICROSMG_VARMOD_LUXE, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_SMG_VARMOD_LUXE, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_ASSAULTSMG_VARMOD_LOWRIDER, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_PUMPSHOTGUN_VARMOD_LOWRIDER, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_SAWNOFFSHOTGUN_VARMOD_LUXE, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_ASSAULTRIFLE_VARMOD_LUXE, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_CARBINERIFLE_VARMOD_LUXE, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_ADVANCEDRIFLE_VARMOD_LUXE, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_SPECIALCARBINE_VARMOD_LOWRIDER, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_BULLPUPRIFLE_VARMOD_LOW, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_MG_VARMOD_LOWRIDER, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_COMBATMG_VARMOD_LOWRIDER, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_SNIPERRIFLE_VARMOD_LUXE, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_MARKSMANRIFLE_VARMOD_LUXE, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_PISTOL_EXTENDED_CLIP, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_PISTOL_FLASHLIGHT, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_PISTOL_SUPPRESSOR, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_SMG_EXTENDED_CLIP, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_SMG_FLASHLIGHT, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_SMG_SUPPRESSOR, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_SMG_SCOPE1, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_SMG_GRIP, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_SHOTGUN_EXTENDED_CLIP, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_SHOTGUN_FLASHLIGHT, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_SHOTGUN_SUPPRESSOR, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_SHOTGUN_GRIP, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_RIFLE_EXTENDED_CLIP, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_RIFLE_FLASHLIGHT, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_RIFLE_SUPPRESSOR, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_RIFLE_SCOPE1, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_RIFLE_GRIP, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_MG_EXTENDED_CLIP, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_MG_SCOPE1, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_MG_GRIP, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_SNIPER_EXTENDED_CLIP, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_SNIPER_SUPPRESSOR, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_SNIPER_FLASHLIGHT, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_SNIPER_SCOPE1, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_SNIPER_SCOPE2, new List<ActionData>() },
        { (int)ItemCatalogIds.COMPONENT_SNIPER_GRIP, new List<ActionData>() },
        { (int)ItemCatalogIds.DRUG_MARIJUANA, new List<ActionData>() },
        { (int)ItemCatalogIds.DRUG_COCAINE, new List<ActionData>() },
        { (int)ItemCatalogIds.DRUG_MDMA, new List<ActionData>() },
        { (int)ItemCatalogIds.DRUG_XANAX, new List<ActionData>() },
        { (int)ItemCatalogIds.DRUG_CODEINE, new List<ActionData>() },
        { (int)ItemCatalogIds.DRUG_METH, new List<ActionData>() },
        { (int)ItemCatalogIds.POLICE_TICKET, new List<ActionData> { new("Ticket anschauen", "ticket:show") } },
        { (int)ItemCatalogIds.LOCKPICK, new List<ActionData>() }
    };

    public static List<ActionData> Get(ItemCatalogIds id)
    {
        return Actions[(int)id];
    }
}