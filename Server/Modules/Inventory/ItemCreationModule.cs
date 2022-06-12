using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Inventory;
using Server.Database.Models.Inventory.Phone;
using Server.Modules.Phone;
using Server.Modules.Weapon;

namespace Server.Modules.Inventory;

public class ItemCreationModule : ITransientScript
{
    private readonly AmmoModule _ammoModule;

    private readonly InventoryModule _inventoryModule;
    private readonly ItemCatalogService _itemCatalogService;
    private readonly ItemService _itemService;
    private readonly ILogger<ItemCreationModule> _logger;
    private readonly PhoneCallModule _phoneCallModule;
    private readonly PhoneModule _phoneModule;
    private readonly WeaponModule _weaponModule;

    public ItemCreationModule(ILogger<ItemCreationModule> logger, ItemCatalogService itemCatalogService,
        ItemService itemService, InventoryModule inventoryModule, PhoneModule phoneModule, WeaponModule weaponModule,
        AmmoModule ammoModule, PhoneCallModule phoneCallModule)
    {
        _logger = logger;
        _itemCatalogService = itemCatalogService;
        _itemService = itemService;

        _inventoryModule = inventoryModule;
        _phoneModule = phoneModule;
        _weaponModule = weaponModule;
        _ammoModule = ammoModule;
        _phoneCallModule = phoneCallModule;
    }

    public async Task<ItemModel?> AddItemAsync(ServerPlayer player, ItemCatalogIds catalogId, int amount,
        int? condition = null, string? customData = null, string? note = null, bool asNew = true, bool isBought = true,
        bool isStolen = false, int? slot = null, ItemState itemState = ItemState.NOT_EQUIPPED)
    {
        var createdItem = await AddItemAsync(player.CharacterModel.InventoryModel, catalogId, amount, condition,
            customData, note, asNew, isBought, isStolen, slot, itemState);

        if (createdItem == null)
        {
            return null;
        }

        if (asNew)
        {
            createdItem = await HandleAddSpecialItems(player, createdItem);
            await _itemService.Update(createdItem);
        }
        else
        {
            await HandleGiveSpecialItems(player, createdItem);
        }

        await _inventoryModule.UpdateInventoryUiAsync(player);

        return createdItem;
    }

    /// <summary>
    ///     Add a new item to the inventory but can return null when the inventory is full.
    /// </summary>
    /// <param name="inventoryModel"></param>
    /// <param name="catalogId"></param>
    /// <param name="amount"></param>
    /// <param name="condition"></param>
    /// <param name="customData"></param>
    /// <param name="note"></param>
    /// <param name="asNew"></param>
    /// <param name="slot"></param>
    /// <returns>New added item that are already saved to the database.</returns>
    public async Task<ItemModel?> AddItemAsync(InventoryModel inventoryModel, ItemCatalogIds catalogId, int amount,
        int? condition = null, string? customData = "", string? note = "", bool asNew = true, bool isBought = true,
        bool isStolen = false, int? slot = null, ItemState itemState = ItemState.NOT_EQUIPPED)
    {
        if (slot != null)
        {
            return await AddItemAsync(inventoryModel, slot.Value, catalogId, amount, condition, customData, note,
                isBought, isStolen, itemState);
        }

        var catalogItem = await _itemCatalogService.GetByKey(catalogId);
        int? freeSlot;

        if (catalogItem.Stackable)
        {
            freeSlot = await _inventoryModule.GetFreeNextSlot(inventoryModel.Id, catalogItem.Weight);
        }
        else
        {
            freeSlot = await _inventoryModule.GetFreeNextSlot(inventoryModel.Id, catalogItem.Weight * amount);
        }

        if (freeSlot == null)
        {
            return null;
        }

        slot = freeSlot;

        return await AddItemAsync(inventoryModel, slot.Value, catalogId, amount, condition, customData, note, isBought,
            isStolen, itemState);
    }

    public async Task HandleGiveSpecialItems(ServerPlayer player, ItemModel itemModel)
    {
        if (itemModel.CatalogItemModelId == ItemCatalogIds.PHONE)
        {
            await _phoneModule.SetOwner(player, itemModel.Id);
        }
        else if (WeaponModule.IsItemWeapon(itemModel.CatalogItemModelId))
        {
            var itemWeapon = itemModel as ItemWeaponModel;

            _weaponModule.Give(player, WeaponModule.GetModelFromId(itemModel.CatalogItemModelId), false,
                itemWeapon.Amount);
            if (itemWeapon.ComponentHashes != null)
            {
                foreach (var componentHash in itemWeapon.ComponentHashes)
                {
                    await AttachmentModule.AddWeaponComponent(player, itemWeapon, componentHash);
                }
            }
        }
        else if (AmmoModule.IsItemAmmo(itemModel.CatalogItemModelId))
        {
            _ammoModule.Give(player, itemModel.CatalogItemModelId, itemModel.Amount);
        }
    }

    public async Task HandleRemoveSpecialItems(ServerPlayer player, ItemModel itemModel)
    {
        if (!player.Exists)
        {
            return;
        }

        if (itemModel is ItemPhoneModel itemPhone)
        {
            _phoneCallModule.Hangup(player, true);
            _phoneCallModule.DenyCall(player);
            await _phoneModule.HandleDropPhoneItem(player, itemPhone);
        }

        if (WeaponModule.IsItemWeapon(itemModel.CatalogItemModelId))
        {
            _weaponModule.Remove(player, WeaponModule.GetModelFromId(itemModel.CatalogItemModelId));
        }
        else if (AmmoModule.IsItemAmmo(itemModel.CatalogItemModelId))
        {
            _ammoModule.Remove(player, itemModel.CatalogItemModelId, itemModel.Amount);
        }
    }

    private async Task<ItemModel> AddItemAsync(InventoryModel inventoryModel, int slot, ItemCatalogIds catalogId,
        int amount, int? condition, string? customData = "", string? note = "", bool isBought = true,
        bool isStolen = false, ItemState itemState = ItemState.NOT_EQUIPPED)
    {
        var existingItem = inventoryModel.Items.FirstOrDefault(i => i.CatalogItemModelId == catalogId);

        if (existingItem != null && existingItem.CatalogItemModel.Stackable &&
            (existingItem.IsBought || !isBought && !existingItem.IsBought))
        {
            existingItem.Amount += amount;
            await _itemService.Update(existingItem);
            return existingItem;
        }

        ItemModel createdItemModel = null;

        switch (catalogId)
        {
            case ItemCatalogIds.NONE:
                break;
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
                createdItemModel = await _itemService.Add(new ItemWeaponModel
                {
                    InventoryModelId = inventoryModel.Id,
                    Slot = slot,
                    CatalogItemModelId = catalogId,
                    Amount = amount,
                    CustomData = customData,
                    Note = note,
                    Condition = condition,
                    ItemState = itemState,
                    IsBought = isBought,
                    IsStolen = isStolen,
                    SerialNumber = WeaponModule.CreateSerialNumber(),
                    InitialOwnerId = inventoryModel.CharacterModelId ?? -1
                });
                break;
            case ItemCatalogIds.NON_ALC_DRINKS:
            case ItemCatalogIds.ALC_DRINKS:
            case ItemCatalogIds.FAST_FOOD:
            case ItemCatalogIds.HEALTHY_FOOD:
            case ItemCatalogIds.BREAD:
            case ItemCatalogIds.SANDWICH:
            case ItemCatalogIds.SOUP:
            case ItemCatalogIds.MEAT:
            case ItemCatalogIds.SWEETS:
            case ItemCatalogIds.CANDY:
                createdItemModel = await _itemService.Add(new ItemFoodModel
                {
                    InventoryModelId = inventoryModel.Id,
                    Slot = slot,
                    CatalogItemModelId = catalogId,
                    Amount = amount,
                    CustomData = customData,
                    Note = note,
                    Condition = condition,
                    ItemState = itemState,
                    IsBought = isBought,
                    IsStolen = isStolen
                });
                break;
            case ItemCatalogIds.CLOTHING_BACKPACK:
                createdItemModel = await _itemService.Add(new ItemClothModel
                {
                    InventoryModelId = inventoryModel.Id,
                    Slot = slot,
                    CatalogItemModelId = catalogId,
                    Amount = amount,
                    CustomData = customData,
                    Note = note,
                    Condition = condition,
                    ItemState = itemState,
                    IsBought = isBought,
                    IsStolen = isStolen,
                    ClothingInventoryModel = new InventoryModel
                    {
                        Name = "Rucksack", InventoryType = InventoryType.CLOTHINGS, MaxWeight = 15
                    }
                });
                break;
            case ItemCatalogIds.CLOTHING_BODY_ARMOR:
                createdItemModel = await _itemService.Add(new ItemClothModel
                {
                    InventoryModelId = inventoryModel.Id,
                    Slot = slot,
                    CatalogItemModelId = catalogId,
                    Amount = amount,
                    CustomData = customData,
                    Note = note,
                    Condition = condition,
                    ItemState = itemState,
                    IsBought = isBought,
                    IsStolen = isStolen,
                    ClothingInventoryModel = new InventoryModel
                    {
                        Name = "Schutzweste", InventoryType = InventoryType.CLOTHINGS, MaxWeight = 12
                    }
                });
                break;
            case ItemCatalogIds.CLOTHING_HAT:
            case ItemCatalogIds.CLOTHING_GLASSES:
            case ItemCatalogIds.CLOTHING_EARS:
            case ItemCatalogIds.CLOTHING_MASK:
            case ItemCatalogIds.CLOTHING_TOP:
            case ItemCatalogIds.CLOTHING_UNDERSHIRT:
            case ItemCatalogIds.CLOTHING_ACCESSORIES:
            case ItemCatalogIds.CLOTHING_WATCH:
            case ItemCatalogIds.CLOTHING_BRACELET:
            case ItemCatalogIds.CLOTHING_PANTS:
            case ItemCatalogIds.CLOTHING_SHOES:
                createdItemModel = await _itemService.Add(new ItemClothModel
                {
                    InventoryModelId = inventoryModel.Id,
                    Slot = slot,
                    CatalogItemModelId = catalogId,
                    Amount = amount,
                    CustomData = customData,
                    Note = note,
                    Condition = condition,
                    ItemState = itemState,
                    IsBought = isBought,
                    IsStolen = isStolen
                });
                break;
            case ItemCatalogIds.PHONE:
                createdItemModel = await _itemService.Add(new ItemPhoneModel
                {
                    InventoryModelId = inventoryModel.Id,
                    Slot = slot,
                    CatalogItemModelId = catalogId,
                    Amount = amount,
                    Note = note,
                    Condition = condition,
                    ItemState = itemState,
                    IsBought = isBought,
                    IsStolen = isStolen,
                    BackgroundImageId = 0,
                    InitialOwnerId = inventoryModel.CharacterModelId ?? -1,
                    CurrentOwnerId = inventoryModel.CharacterModelId ?? null,
                    PhoneNumber = await _phoneModule.GetRandomPhoneNumber(),
                    LastTimeOpenedNotifications = DateTime.Now,
                    Contacts = new List<PhoneContactModel>(),
                    Chats = new List<PhoneChatModel>(),
                    Notifications = new List<PhoneNotificationModel>()
                });
                break;
            case ItemCatalogIds.KEY:
                createdItemModel = await _itemService.Add(new ItemKeyModel
                {
                    InventoryModelId = inventoryModel.Id,
                    Slot = slot,
                    CatalogItemModelId = catalogId,
                    Amount = amount,
                    CustomData = customData,
                    Note = note,
                    Condition = condition,
                    ItemState = itemState,
                    IsBought = isBought,
                    IsStolen = isStolen
                });
                break;
            case ItemCatalogIds.GROUP_KEY:
                createdItemModel = await _itemService.Add(new ItemGroupKeyModel
                {
                    InventoryModelId = inventoryModel.Id,
                    Slot = slot,
                    CatalogItemModelId = catalogId,
                    Amount = amount,
                    CustomData = customData,
                    Note = note,
                    Condition = condition,
                    ItemState = itemState,
                    IsBought = isBought,
                    IsStolen = isStolen
                });
                break;
            case ItemCatalogIds.RADIO:
                createdItemModel = await _itemService.Add(new ItemRadioModel
                {
                    InventoryModelId = inventoryModel.Id,
                    Slot = slot,
                    CatalogItemModelId = catalogId,
                    Amount = amount,
                    CustomData = customData,
                    Note = note,
                    Condition = condition,
                    ItemState = itemState,
                    IsBought = isBought,
                    IsStolen = isStolen
                });
                break;
            case ItemCatalogIds.HANDCUFF:
                createdItemModel = await _itemService.Add(new ItemHandCuffModel
                {
                    InventoryModelId = inventoryModel.Id,
                    Slot = slot,
                    CatalogItemModelId = catalogId,
                    Amount = amount,
                    CustomData = customData,
                    Note = note,
                    Condition = condition,
                    ItemState = itemState,
                    IsBought = isBought,
                    IsStolen = isStolen
                });
                break;
            case ItemCatalogIds.DOLLAR:
            case ItemCatalogIds.LICENSES:
            case ItemCatalogIds.REPAIR_KIT:
            case ItemCatalogIds.MEGAPHONE:
            case ItemCatalogIds.HANDCUFF_KEY:
            case ItemCatalogIds.TRAFFIC_CONE:
                createdItemModel = await _itemService.Add(new ItemModel
                {
                    InventoryModelId = inventoryModel.Id,
                    Slot = slot,
                    CatalogItemModelId = catalogId,
                    Amount = amount,
                    CustomData = customData,
                    Note = note,
                    Condition = condition,
                    ItemState = itemState,
                    IsBought = isBought,
                    IsStolen = isStolen
                });
                break;
            case ItemCatalogIds.AMMO_PISTOL:
            case ItemCatalogIds.AMMO_MACHINE_GUN:
            case ItemCatalogIds.AMMO_ASSAULT:
            case ItemCatalogIds.AMMO_SNIPER:
            case ItemCatalogIds.AMMO_SHOTGUN:
            case ItemCatalogIds.AMMO_LIGHT_MACHINE_GUN:
                createdItemModel = await _itemService.Add(new ItemWeaponAmmoModel
                {
                    InventoryModelId = inventoryModel.Id,
                    Slot = slot,
                    CatalogItemModelId = catalogId,
                    Amount = amount,
                    CustomData = customData,
                    Note = note,
                    Condition = condition,
                    ItemState = itemState,
                    IsBought = isBought,
                    IsStolen = isStolen
                });
                break;
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
                createdItemModel = await _itemService.Add(new ItemWeaponAttachmentModel
                {
                    InventoryModelId = inventoryModel.Id,
                    Slot = slot,
                    CatalogItemModelId = catalogId,
                    Amount = amount,
                    CustomData = customData,
                    Note = note,
                    Condition = condition,
                    ItemState = itemState,
                    IsBought = isBought,
                    IsStolen = isStolen
                });
                break;
            case ItemCatalogIds.DRUG_MARIJUANA:
            case ItemCatalogIds.DRUG_COCAINE:
            case ItemCatalogIds.DRUG_MDMA:
            case ItemCatalogIds.DRUG_XANAX:
            case ItemCatalogIds.DRUG_CODEINE:
            case ItemCatalogIds.DRUG_METH:
                createdItemModel = await _itemService.Add(new ItemDrugModel
                {
                    InventoryModelId = inventoryModel.Id,
                    Slot = slot,
                    CatalogItemModelId = catalogId,
                    Amount = amount,
                    CustomData = customData,
                    Note = note,
                    Condition = condition,
                    ItemState = itemState,
                    IsBought = isBought,
                    IsStolen = isStolen
                });
                break;
            case ItemCatalogIds.POLICE_TICKET:
                createdItemModel = await _itemService.Add(new ItemPoliceTicketModel
                {
                    InventoryModelId = inventoryModel.Id,
                    Slot = slot,
                    CatalogItemModelId = catalogId,
                    Amount = amount,
                    CustomData = customData,
                    Note = note,
                    Condition = condition,
                    ItemState = itemState,
                    IsBought = isBought,
                    IsStolen = isStolen
                });
                break;
            case ItemCatalogIds.LOCKPICK:
                createdItemModel = await _itemService.Add(new ItemLockPickModel
                {
                    InventoryModelId = inventoryModel.Id,
                    Slot = slot,
                    CatalogItemModelId = catalogId,
                    Amount = amount,
                    CustomData = customData,
                    Note = note,
                    Condition = condition,
                    ItemState = itemState,
                    IsBought = isBought,
                    IsStolen = isStolen
                });
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(catalogId), catalogId, null);
        }

        return createdItemModel;
    }

    private async Task<ItemModel> HandleAddSpecialItems(ServerPlayer player, ItemModel newItemModel)
    {
        if (newItemModel.CatalogItemModelId == ItemCatalogIds.LICENSES)
        {
            newItemModel.CustomData = player.CharacterModel.Id.ToString();
        }
        else if (WeaponModule.IsItemWeapon(newItemModel.CatalogItemModelId))
        {
            _weaponModule.Give(player, WeaponModule.GetModelFromId(newItemModel.CatalogItemModelId), true,
                newItemModel.Amount);
        }
        else if (AmmoModule.IsItemAmmo(newItemModel.CatalogItemModelId))
        {
            _ammoModule.Give(player, newItemModel.CatalogItemModelId, newItemModel.Amount);
        }

        return newItemModel;
    }
}