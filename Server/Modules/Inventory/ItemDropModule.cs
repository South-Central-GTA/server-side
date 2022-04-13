using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Inventory;
using Server.Helper;
using Server.Modules.Clothing;

namespace Server.Modules.Inventory;

public class ItemDropModule
    : ITransientScript
{
    private readonly InventoryModule _inventoryModule;
    private readonly ItemCreationModule _itemCreationModule;

    private readonly ItemService _itemService;
    private readonly ILogger<ItemDropModule> _logger;
    private readonly Serializer _serializer;

    public ItemDropModule(
        ILogger<ItemDropModule> logger,
        Serializer serializer,
        ItemService itemService,
        InventoryModule inventoryModule,
        ItemCreationModule itemCreationModule)
    {
        _logger = logger;
        _serializer = serializer;

        _itemService = itemService;

        _inventoryModule = inventoryModule;
        _itemCreationModule = itemCreationModule;
    }

    public async Task<bool> Pickup(ServerPlayer player, int itemId)
    {
        var item = await _itemService.GetByKey(itemId);

        var sameItemType = player.CharacterModel.InventoryModel.Items.Find(i => i.CatalogItemModelId == item.CatalogItemModelId
                                                                      && item.CatalogItemModel.Stackable);

        if (player.Position.Distance(item.Position) > 3.0f)
        {
            player.SendNotification("Du bist zu weit entfernt.", NotificationType.ERROR);
            return false;
        }

        int? freeSlot = null;

        if (item is ItemWeaponModel itemWeapon)
        {
            var items = await _itemService.GetAll();
            var weaponAttachments = items.Where(i => i is ItemWeaponAttachmentModel weaponAttachment
                                                     && weaponAttachment.ItemWeaponId == itemWeapon.Id).ToList();

            if (weaponAttachments.Count != 0)
            {
                var weight = weaponAttachments.Sum(i => i.CatalogItemModel.Weight);

                freeSlot = await _inventoryModule.GetFreeNextSlot(player.CharacterModel.InventoryModel.Id, weight);
                if (!freeSlot.HasValue)
                {
                    player.SendNotification("Dein Charakter hat nicht genug Platz im Inventar für die Waffe samt Erweiterungen.", NotificationType.ERROR);
                    return false;
                }

                foreach (var weaponAttachment in weaponAttachments)
                {
                    weaponAttachment.ItemState = ItemState.NOT_EQUIPPED;
                    weaponAttachment.InventoryModelId = player.CharacterModel.InventoryModel.Id;
                }

                await _itemService.UpdateRange(weaponAttachments);
            }
            else
            {
                freeSlot = await _inventoryModule.GetFreeNextSlot(player.CharacterModel.InventoryModel.Id, item.CatalogItemModel.Weight);
            }
        }
        else
        {
            freeSlot = await _inventoryModule.GetFreeNextSlot(player.CharacterModel.InventoryModel.Id, item.CatalogItemModel.Weight);
        }

        if (!freeSlot.HasValue)
        {
            player.SendNotification("Dein Charakter hat nicht genug Platz im Inventar.", NotificationType.ERROR);
            return false;
        }

        if (sameItemType != null)
        {
            sameItemType.Amount += item.Amount;

            await _itemService.Update(sameItemType);
            await _itemService.Remove(item);
        }
        else
        {
            item.ItemState = ItemState.NOT_EQUIPPED;
            item.InventoryModelId = player.CharacterModel.InventoryModel.Id;
            item.Slot = freeSlot.Value;
            item.Position = new Vector3(0, 0, 0);
            item.DroppedByCharacter = null;

            await _itemService.Update(item);
        }

        await _itemCreationModule.HandleGiveSpecialItems(player, item);

        if (ClothingModule.IsClothesOrProp(item.CatalogItemModelId))
        {
            var data = _serializer.Deserialize<ClothingData>(item.CustomData);
            player.SendNotification($"Dein Charakter hat {item.Amount}x {data.Title} aufgehoben.", NotificationType.SUCCESS);
        }
        else
        {
            player.SendNotification($"Dein Charakter hat {item.Amount}x {item.CatalogItemModel.Name} aufgehoben.", NotificationType.SUCCESS);
        }

        return true;
    }

    public async Task PutOn(ServerPlayer player, int itemId)
    {
        var item = await _itemService.GetByKey(itemId);

        var sameItem = player.CharacterModel.InventoryModel.Items.Find(i => i.CatalogItemModel.Id == item.CatalogItemModel.Id
                                                                  && i.ItemState == ItemState.EQUIPPED);

        if (sameItem != null)
        {
            player.SendNotification("Dein Charakter hat dieses Kleidungsstück schon an.", NotificationType.ERROR);
            return;
        }

        var clothingData = _serializer.Deserialize<ClothingData>(item.CustomData);
        if (clothingData.GenderType != player.CharacterModel.Gender)
        {
            player.SendNotification("Dein Charakter kann keine Kleidung des anderen Geschlechtes anziehen.", NotificationType.ERROR);
            return;
        }

        item.ItemState = ItemState.EQUIPPED;
        item.InventoryModelId = player.CharacterModel.InventoryModel.Id;
        item.Position = new Vector3(0, 0, 0);
        item.DroppedByCharacter = null;

        await _itemService.Update(item);
    }
}