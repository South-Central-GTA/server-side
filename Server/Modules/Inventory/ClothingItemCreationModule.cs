using System;
using System.Linq;
using System.Threading.Tasks;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Inventory;
using Server.Modules.Inventory.Exceptions;
using Server.Modules.Weapon;

namespace Server.Modules.Inventory;

public class ClothingItemCreationModule : ITransientScript
{
    private readonly AmmoModule _ammoModule;
    private readonly InventoryModule _inventoryModule;

    private readonly ItemCatalogService _itemCatalogService;
    private readonly ItemService _itemService;

    public ClothingItemCreationModule(ItemService itemService, ItemCatalogService itemCatalogService, 
        InventoryModule inventoryModule, AmmoModule ammoModule)
    {
        _itemService = itemService;
        _itemCatalogService = itemCatalogService;
        _inventoryModule = inventoryModule;
        _ammoModule = ammoModule;
    }

    public async Task<ItemModel> AddItemAsync(InventoryModel inventoryModel, ItemCatalogIds catalogId,
        ClothingData clothingData, int amount = 1, string? note = null, bool isBought = true,
        bool isStolen = false, ItemState itemState = ItemState.EQUIPPED)
    {
        var catalogItem = await _itemCatalogService.GetByKey(catalogId);
        if (catalogItem == null)
        {
            throw new MissingCatalogItemException();
        }
        
        int? freeSlot;

        if (catalogItem.Stackable)
        {
            freeSlot = await _inventoryModule.GetFreeNextSlot(inventoryModel.Id, catalogItem.Weight);
        }
        else
        {
            freeSlot = await _inventoryModule.GetFreeNextSlot(inventoryModel.Id, catalogItem.Weight * amount);
        }

        if (!freeSlot.HasValue)
        {
            throw new InventoryFullException();
        }

        var existingItem = inventoryModel.Items.FirstOrDefault(i => i.CatalogItemModelId == catalogId);

        if (existingItem != null && existingItem.CatalogItemModel.Stackable &&
            (existingItem.IsBought || !isBought && !existingItem.IsBought))
        {
            existingItem.Amount += amount;
            await _itemService.Update(existingItem);
            return existingItem;
        }

        ItemModel createdItemModel;

        switch (catalogId)
        {
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
                    Slot = freeSlot.Value,
                    CatalogItemModelId = catalogId,
                    Amount = amount,
                    Note = note,
                    ItemState = itemState,
                    IsBought = isBought,
                    IsStolen = isStolen,
                    GenderType = clothingData.GenderType,
                    DrawableId = clothingData.DrawableId,
                    TextureId = clothingData.TextureId,
                    Title = clothingData.Title,
                });
                break;
            case ItemCatalogIds.CLOTHING_BACKPACK:
                createdItemModel = await _itemService.Add(new ItemClothModel
                {
                    InventoryModelId = inventoryModel.Id,
                    Slot = freeSlot.Value,
                    CatalogItemModelId = catalogId,
                    Amount = amount,
                    Note = note,
                    ItemState = itemState,
                    IsBought = isBought,
                    IsStolen = isStolen,
                    GenderType = clothingData.GenderType,
                    DrawableId = clothingData.DrawableId,
                    TextureId = clothingData.TextureId,
                    Title = clothingData.Title,
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
                    Slot = freeSlot.Value,
                    CatalogItemModelId = catalogId,
                    Amount = amount,
                    Note = note,
                    ItemState = itemState,
                    IsBought = isBought,
                    IsStolen = isStolen,
                    GenderType = clothingData.GenderType,
                    DrawableId = clothingData.DrawableId,
                    TextureId = clothingData.TextureId,
                    Title = clothingData.Title,
                    ClothingInventoryModel = new InventoryModel
                    {
                        Name = "Schutzweste", InventoryType = InventoryType.CLOTHINGS, MaxWeight = 12
                    }
                });
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(catalogId), catalogId, "Item is no clothing item.");
        }

        return createdItemModel;
    }
}