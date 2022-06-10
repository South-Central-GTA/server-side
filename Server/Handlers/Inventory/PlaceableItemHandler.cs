using System;
using System.Linq;
using AltV.Net.Async;
using AltV.Net.Data;
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
using Server.Modules.Clothing;
using Server.Modules.EntitySync;
using Server.Modules.Inventory;

namespace Server.Handlers.Inventory;

public class PlaceableItemHandler : ISingletonScript
{
    private readonly AntiCheatModule _antiCheatModule;

    private readonly InventoryModule _inventoryModule;
    private readonly ItemCreationModule _itemCreationModule;

    private readonly ItemDestructionModule _itemDestructionModule;
    private readonly ItemDropModule _itemDropModule;

    private readonly ItemService _itemService;
    private readonly ObjectSyncModule _objectSyncModule;
    private readonly Serializer _serializer;

    public PlaceableItemHandler(Serializer serializer, ItemService itemService, InventoryModule inventoryModule,
        ItemDropModule itemDropModule, AntiCheatModule antiCheatModule, ItemCreationModule itemCreationModule,
        ObjectSyncModule objectSyncModule, ItemDestructionModule itemDestructionModule)
    {
        _serializer = serializer;

        _itemService = itemService;

        _inventoryModule = inventoryModule;
        _itemDropModule = itemDropModule;
        _antiCheatModule = antiCheatModule;
        _itemCreationModule = itemCreationModule;
        _objectSyncModule = objectSyncModule;
        _itemDestructionModule = itemDestructionModule;

        AltAsync.OnClient<ServerPlayer, string>("placeableitem:place", OnPlayerDropItem);
        AltAsync.OnClient<ServerPlayer, ulong>("placeableitem:pickup", OnPlayerPickupItem);
        AltAsync.OnClient<ServerPlayer, ulong>("placeableitem:puton", OnPlayerPutOnItem);
        AltAsync.OnClient<ServerPlayer, ulong>("placeableitem:deleteitem", OnPlayerDeleteItem);
    }

    private async void OnPlayerDropItem(ServerPlayer player, string itemDropJson)
    {
        if (!player.Exists)
        {
            return;
        }

        if (player.CharacterModel.DeathState == DeathState.DEAD)
        {
            player.SendNotification("Dein Charakter kann jetzt keine Items platzieren.", NotificationType.ERROR);
            return;
        }

        var dropItemData = _serializer.Deserialize<DropItemData>(itemDropJson);

        var items = await _itemService.GetAll();
        var itemOnGround = items.Find(i =>
            dropItemData.Position.Distance(i.Position) <= 0.3f && i.ItemState == ItemState.DROPPED);

        if (itemOnGround != null)
        {
            player.SendNotification("Hier liegt schon ein Item auf dem Boden.", NotificationType.ERROR);
            return;
        }

        if (!_antiCheatModule.DetectDropItemPositionHack(player, dropItemData.Position))
        {
            return;
        }

        var item = items.Find(i => i.Id == dropItemData.ItemId);
        if (item == null)
        {
            player.SendNotification("Item konnte nicht gefunden werden.", NotificationType.ERROR);
            return;
        }

        if (!item.IsBought)
        {
            player.SendNotification("Dies würde dein Charakter nicht tun.", NotificationType.ERROR);
            return;
        }

        if (item is ItemWeaponModel itemWeapon)
        {
            var weaponAttachments = items.Where(i =>
                    i is ItemWeaponAttachmentModel weaponAttachment && weaponAttachment.ItemWeaponId == itemWeapon.Id)
                .ToList();

            foreach (var weaponAttachment in weaponAttachments)
            {
                weaponAttachment.ItemState = ItemState.DROPPED;
                weaponAttachment.InventoryModelId = null;
            }

            await _itemService.UpdateRange(weaponAttachments);
        }

        await _itemCreationModule.HandleRemoveSpecialItems(player, item);
        if (ClothingModule.IsClothesOrProp(item.CatalogItemModelId) && item.CustomData != null)
        {
            var data = _serializer.Deserialize<ClothingData>(item.CustomData);
            player.SendNotification($"Dein Charakter hat {item.Amount}x {data.Title} abgelegt.",
                NotificationType.SUCCESS);
        }
        else
        {
            player.SendNotification($"Dein Charakter hat {item.Amount}x {item.CatalogItemModel.Name} abgelegt.",
                NotificationType.SUCCESS);
        }

        item.ItemState = ItemState.DROPPED;
        item.Position = dropItemData.Position;
        item.Rotation = player.Rotation;
        item.Dimension = player.Dimension;
        item.Slot = null;
        item.InventoryModelId = null;
        item.DroppedByCharacter = player.CharacterModel.Name;

        await _itemService.Update(item);
        await _inventoryModule.UpdateInventoryUiAsync(player);

        _objectSyncModule.Create(item.CatalogItemModel.Model, item.CatalogItemModel.Name,
            new Position(player.Position.X, player.Position.Y, player.Position.Z - 1 + item.CatalogItemModel.ZOffset),
            item.CatalogItemModel.Rotation, player.Dimension, 200, true, false, item.Id, item.DroppedByCharacter,
            _serializer.Serialize(DateTime.Now));

        if (ClothingModule.IsClothesOrProp(item.CatalogItemModelId))
        {
            player.UpdateClothes();
        }
    }

    private async void OnPlayerPickupItem(ServerPlayer player, ulong objectId)
    {
        if (!player.Exists)
        {
            return;
        }

        if (player.CharacterModel.DeathState == DeathState.DEAD)
        {
            player.SendNotification("Dein Charakter kann jetzt keine Items aufheben.", NotificationType.ERROR);
            return;
        }

        var serverObject = _objectSyncModule.Get(objectId);
        if (serverObject == null)
        {
            return;
        }

        var item = await _itemService.GetByKey(serverObject.ItemId);
        if (item == null)
        {
            player.SendNotification("Item konnte nicht gefunden werden.", NotificationType.ERROR);
            return;
        }

        if (!await _inventoryModule.CanCarry(player, item.CatalogItemModel.Id, item.Amount))
        {
            return;
        }

        if (_antiCheatModule.DetectPickupItemHack(item))
        {
            return;
        }

        var success = await _itemDropModule.Pickup(player, serverObject.ItemId);
        if (success)
        {
            await _inventoryModule.UpdateInventoryUiAsync(player);

            _objectSyncModule.Delete(objectId);
        }
    }

    private async void OnPlayerPutOnItem(ServerPlayer player, ulong objectId)
    {
        if (!player.Exists)
        {
            return;
        }

        if (player.CharacterModel.DeathState == DeathState.DEAD)
        {
            player.SendNotification("Dein Charakter kann jetzt keine Items aufheben.", NotificationType.ERROR);
            return;
        }

        var serverObject = _objectSyncModule.Get(objectId);
        if (serverObject == null)
        {
            return;
        }

        var item = await _itemService.GetByKey(serverObject.ItemId);
        if (item == null)
        {
            player.SendNotification("Item konnte nicht gefunden werden.", NotificationType.ERROR);
            return;
        }

        if (!await _inventoryModule.CanCarry(player, item.CatalogItemModel.Id, item.Amount))
        {
            return;
        }

        await _itemDropModule.PutOn(player, serverObject.ItemId);
        await _inventoryModule.UpdateInventoryUiAsync(player);
        _objectSyncModule.Delete(objectId);
        player.UpdateClothes();
    }

    private async void OnPlayerDeleteItem(ServerPlayer player, ulong objectId)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!player.IsAduty)
        {
            return;
        }

        var serverObject = _objectSyncModule.Get(objectId);
        if (serverObject == null)
        {
            return;
        }

        await _itemDestructionModule.Destroy(serverObject.ItemId);

        player.SendNotification("Du hast das Item administrativ gelöscht.", NotificationType.INFO);

        _objectSyncModule.Delete(objectId);
    }
}