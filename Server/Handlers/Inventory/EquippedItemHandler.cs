using System.Linq;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Helper;
using Server.Modules.Inventory;

namespace Server.Handlers.Inventory;

public class EquippedItemHandler : ISingletonScript
{
    private readonly InventoryModule _inventoryModule;
    private readonly InventoryService _inventoryService;

    private readonly ItemService _itemService;
    private readonly Serializer _serializer;

    public EquippedItemHandler(
        Serializer serializer,
        ItemService itemService,
        InventoryService inventoryService,
        InventoryModule inventoryModule)
    {
        _serializer = serializer;

        _itemService = itemService;
        _inventoryService = inventoryService;

        _inventoryModule = inventoryModule;

        AltAsync.OnClient<ServerPlayer, int>("item:unequip", OnPlayerUnequipItem);
        AltAsync.OnClient<ServerPlayer, int>("item:equip", OnPlayerEquipItem);
    }

    private async void OnPlayerUnequipItem(ServerPlayer player, int itemId)
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

        var freeSlot = await _inventoryModule.GetFreeNextSlot(player.CharacterModel.InventoryModel.Id);
        if (item.CatalogItemModel.Id == ItemCatalogIds.CLOTHING_TOP)
        {
            player.CharacterModel.Torso = 15;
            player.CharacterModel.TorsoTexture = 0;
        }

        item.ItemState = ItemState.NOT_EQUIPPED;
        item.Slot = freeSlot;

        await _itemService.Update(item);
        await _inventoryModule.UpdateInventoryUiAsync(player);
        player.UpdateClothes();
    }

    private async void OnPlayerEquipItem(ServerPlayer player, int itemId)
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

        if (item.InventoryModelId != player.CharacterModel.InventoryModel.Id)
        {
            if (!await _inventoryModule.CanCarry(player, item.CatalogItemModel.Id))
            {
                return;
            }
        }

        var inv = await _inventoryService.GetByKey(player.CharacterModel.InventoryModel.Id);
        var requestedItems = inv.Items.Where(i => i.CatalogItemModelId == item.CatalogItemModelId && i.ItemState == ItemState.EQUIPPED);
        if (requestedItems.Any())
        {
            player.SendNotification("Dein Charakter trägt schon ein Kleidungsstück dieser Sorte.", NotificationType.ERROR);
            return;
        }

        if (!item.IsBought)
        {
            player.SendNotification("Dies würde dein Charakter nicht tun.", NotificationType.ERROR);
            return;
        }

        var clothingData = _serializer.Deserialize<ClothingData>(item.CustomData);
        if (clothingData.GenderType != player.CharacterModel.Gender)
        {
            player.SendNotification("Dein Charakiter kann keine Kleidung des anderen Geschlechtes anziehen.",
                                    NotificationType.ERROR);
            return;
        }

        item.InventoryModelId = player.CharacterModel.InventoryModel.Id;
        item.ItemState = ItemState.EQUIPPED;
        item.Slot = null;

        await _itemService.Update(item);
        await _inventoryModule.UpdateInventoryUiAsync(player);
        player.UpdateClothes();
    }
}