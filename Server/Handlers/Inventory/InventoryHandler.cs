using System.Collections.Generic;
using System.Linq;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Inventory;
using Server.Helper;
using Server.Modules.Admin;
using Server.Modules.Inventory;
using Server.Modules.Weapon;

namespace Server.Handlers.Inventory;

public class InventoryHandler : ISingletonScript
{
    private readonly AntiCheatModule _antiCheatModule;
    private readonly AttachmentModule _attachmentModule;

    private readonly InventoryModule _inventoryModule;

    private readonly InventoryService _inventoryService;
    private readonly ItemCreationModule _itemCreationModule;
    private readonly ItemService _itemService;
    private readonly Serializer _serializer;


    public InventoryHandler(
        Serializer serializer,
        ItemService itemService,
        InventoryService inventoryService,
        InventoryModule inventoryModule,
        AntiCheatModule antiCheatModule,
        ItemCreationModule itemCreationModule,
        AttachmentModule attachmentModule)
    {
        _serializer = serializer;

        _itemService = itemService;
        _inventoryService = inventoryService;
        _antiCheatModule = antiCheatModule;

        _inventoryModule = inventoryModule;
        _itemCreationModule = itemCreationModule;
        _attachmentModule = attachmentModule;

        AltAsync.OnClient<ServerPlayer>("inventory:close", OnCloseInventory);
        AltAsync.OnClient<ServerPlayer>("inventory:request", OnRequestInventory);
        AltAsync.OnClient<ServerPlayer, int, int>("inventory:swapitem", OnPlayerSwapItem);
        AltAsync.OnClient<ServerPlayer, int, int>("inventory:switchitem", OnPlayerSwitchItem);
        AltAsync.OnClient<ServerPlayer, int, int>("inventory:splititem", OnPlayerSplitItem);
        AltAsync.OnClient<ServerPlayer, int, string>("inventory:noteitem", OnPlayerNoteItem);
        AltAsync.OnClient<ServerPlayer, int, string>("inventory:renameitem", OnPlayerRenameItem);
        AltAsync.OnClient<ServerPlayer, int>("item:consume", OnConsumeItem);
    }

    private async void OnCloseInventory(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        player.DefaultInventories = new List<InventoryModel>();
    }

    private async void OnRequestInventory(ServerPlayer player)
    {
        await _inventoryModule.OpenInventoryUiAsync(player);
    }

    private async void OnPlayerSwapItem(ServerPlayer player, int draggingItemId, int droppedItemId)
    {
        if (!player.Exists)
        {
            return;
        }

        var draggingItem = await _itemService.GetByKey(draggingItemId);
        if (draggingItem == null)
        {
            return;
        }
        
        var droppedItem = await _itemService.GetByKey(droppedItemId);
        if (droppedItem == null)
        {
            return;
        }
        
        if (!draggingItem.IsBought || !droppedItem.IsBought)
        {
            player.SendNotification("Dies würde dein Charakter nicht tun.", NotificationType.ERROR);
            return;
        }

        if (draggingItem.ItemState is ItemState.EQUIPPED or ItemState.FORCE_EQUIPPED)
        {
            return;
        }

        // Check if we are trying to swap the backpack item into an backpack inventory.
        if (draggingItem.CatalogItemModelId == ItemCatalogIds.CLOTHING_BACKPACK && droppedItem.InventoryModel.ItemClothModelId.HasValue
            || droppedItem.CatalogItemModelId == ItemCatalogIds.CLOTHING_BACKPACK && draggingItem.InventoryModel.ItemClothModelId.HasValue)
        {
            player.SendNotification("Du kannst kein Rucksack in einen Rucksack packen.", NotificationType.ERROR);
            return;
        }

        if (draggingItem is ItemWeaponAttachmentModel weaponAttachment
            && droppedItem is ItemWeaponModel weapon)
        {
            await _attachmentModule.AddToWeapon(player, weapon, weaponAttachment);
        }
        else
        {
            if (droppedItem.CatalogItemModel.Stackable
                && draggingItem.CatalogItemModelId == droppedItem.CatalogItemModelId)
            {
                if (_antiCheatModule.DetectStackItemHack(draggingItem, droppedItem))
                {
                    return;
                }

                droppedItem.Amount += draggingItem.Amount;

                await _itemService.Remove(draggingItem);
            }
            else
            {
                if (_antiCheatModule.DetectSwapItemHack(draggingItem, droppedItem))
                {
                    return;
                }

                (draggingItem.Slot, droppedItem.Slot) = (droppedItem.Slot, draggingItem.Slot);
                (draggingItem.InventoryModelId, droppedItem.InventoryModelId) = (droppedItem.InventoryModelId, draggingItem.InventoryModelId);

                await _itemService.Update(draggingItem);
            }

            await _itemService.Update(droppedItem);
        }

        await _inventoryModule.UpdateInventoryUIs(player, draggingItem.InventoryModel);
    }

    private async void OnPlayerSwitchItem(ServerPlayer player, int invId, int itemId)
    {
        if (!player.Exists)
        {
            return;
        }

        var item = await _itemService.GetByKey(itemId);
        if (item == null)
        {
            player.SendNotification("Item konnte nicht gefunden werden.", NotificationType.ERROR);
            return;
        }
        
        var inv = await _inventoryService.GetByKey(invId);
        if (inv == null)
        {
            player.SendNotification("Das ehemalige Inventar existiert nicht mehr.", NotificationType.ERROR);
            return;
        }
        
        if (await _antiCheatModule.DetectSwitchItemHack(player, inv, item))
        {
            return;
        }

        if (item.CatalogItemModelId == ItemCatalogIds.CLOTHING_BACKPACK && inv.ItemClothModelId.HasValue)
        {
            player.SendNotification("Du kannst kein Rucksack in einen Rucksack packen.", NotificationType.ERROR);
            return;
        }

        var result = await _inventoryModule.CanCarry(inv.Id, item.CatalogItemModel.Id, item.Amount);
        switch (result)
        {
            case CanCarryErrorType.LIMIT:
                player.SendNotification($"In einem Inventar können maximal {item.CatalogItemModel.MaxLimit} Stück des Items '{item.CatalogItemModel.Name}' liegen.", NotificationType.ERROR);
                return;
            case CanCarryErrorType.NO_SPACE:
                player.SendNotification("Das Inventar hat nicht genug Platz.", NotificationType.ERROR);
                return;
        }

        var freeSlot = _inventoryModule.GetFreeNextSlot(inv, item.CatalogItemModel.Weight * item.Amount);
        if (freeSlot == null)
        {
            return;
        }

        if (!item.InventoryModelId.HasValue)
        {
            return;
        }

        var oldInv = await _inventoryService.GetByKey(item.InventoryModelId.Value);
        if (oldInv == null)
        {
            player.SendNotification("Das ehemalige Inventar existiert nicht mehr.", NotificationType.ERROR);
            return;
        }

        item.Slot = freeSlot;
        item.InventoryModelId = inv.Id;

        if (item is ItemWeaponModel itemWeapon)
        {
            var items = await _itemService.GetAll();
            var weaponAttachments = items.Where(i => i is ItemWeaponAttachmentModel weaponAttachment
                                                     && weaponAttachment.ItemWeaponId == itemWeapon.Id).ToList();

            foreach (var weaponAttachment in weaponAttachments)
            {
                weaponAttachment.InventoryModelId = inv.Id;
            }

            await _itemService.UpdateRange(weaponAttachments);
        }

        if (player.CharacterModel.InventoryModel.Id == inv.Id)
        {
            await _itemCreationModule.HandleGiveSpecialItems(player, item);
        }
        else
        {
            await _itemCreationModule.HandleRemoveSpecialItems(player, item);
        }

        await _itemService.Update(item);

        await _inventoryModule.UpdateInventoryUIs(player, inv);
        await _inventoryModule.UpdateInventoryUIs(player, oldInv);
    }

    private async void OnPlayerSplitItem(ServerPlayer player, int itemId, int amount)
    {
        if (!player.Exists)
        {
            return;
        }

        var splittedItem = await _itemService.GetByKey(itemId);

        if (!splittedItem.IsBought)
        {
            player.SendNotification("Dies würde dein Charakter nicht tun.", NotificationType.ERROR);
            return;
        }

        var freeSlot = await _inventoryModule.GetFreeNextSlot(player.CharacterModel.InventoryModel.Id);
        await _itemService.Add(new ItemModel(splittedItem.CatalogItemModelId,
                                        freeSlot,
                                        splittedItem.CustomData,
                                        "",
                                        amount,
                                        splittedItem.Condition,
                                        splittedItem.IsBought,
                                        splittedItem.IsStolen,
                                        ItemState.NOT_EQUIPPED) { InventoryModelId = splittedItem.InventoryModelId });

        splittedItem.Amount -= amount;

        await _itemService.Update(splittedItem);
        await _inventoryModule.UpdateInventoryUIs(player, splittedItem.InventoryModel);
    }

    private async void OnPlayerNoteItem(ServerPlayer player, int itemId, string note)
    {
        if (!player.Exists)
        {
            return;
        }

        var item = await _itemService.GetByKey(itemId);

        item.Note = note;

        await _itemService.Update(item);
        await _inventoryModule.UpdateInventoryUIs(player, item.InventoryModel);
    }

    private async void OnPlayerRenameItem(ServerPlayer player, int itemId, string newName)
    {
        if (!player.Exists)
        {
            return;
        }

        var item = await _itemService.GetByKey(itemId);

        var data = _serializer.Deserialize<ClothingData>(item.CustomData);
        data.Title = newName;

        item.CustomData = _serializer.Serialize(data);

        await _itemService.Update(item);
        await _inventoryModule.UpdateInventoryUIs(player, item.InventoryModel);
    }

    private async void OnConsumeItem(ServerPlayer player, int itemId)
    {
        if (!player.Exists)
        {
            return;
        }

        var item = await _itemService.GetByKey(itemId);
        if (item == null)
        {
            return;
        }

        if (!item.IsBought)
        {
            player.SendNotification("Dies würde dein Charakter nicht tun.", NotificationType.ERROR);
            return;
        }

        if (item.Amount > 1)
        {
            item.Amount--;
            await _itemService.Update(item);
        }
        else
        {
            await _itemService.Remove(item);
        }

        player.SendNotification("Du hast das Item konsumiert.", NotificationType.INFO);

        await _inventoryModule.UpdateInventoryUIs(player, item.InventoryModel);
    }
}