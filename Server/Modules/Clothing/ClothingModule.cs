using System.Linq;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Data.Models;
using Server.Database.Enums;
using Server.Database.Models.Inventory;
using Server.Modules.Clothing.Exceptions;
using Server.Modules.Vehicles.Exceptions;

namespace Server.Modules.Clothing;

public class ClothingModule : ITransientScript
{
    public static bool IsClothesOrProp(ItemCatalogIds catalogId)
    {
        return IsClothes(catalogId) || IsProps(catalogId);
    }

    public static bool IsClothes(ItemCatalogIds catalogId)
    {
        switch (catalogId)
        {
            case ItemCatalogIds.CLOTHING_MASK:
            case ItemCatalogIds.CLOTHING_TOP:
            case ItemCatalogIds.CLOTHING_UNDERSHIRT:
            case ItemCatalogIds.CLOTHING_ACCESSORIES:
            case ItemCatalogIds.CLOTHING_PANTS:
            case ItemCatalogIds.CLOTHING_BACKPACK:
            case ItemCatalogIds.CLOTHING_BODY_ARMOR:
            case ItemCatalogIds.CLOTHING_SHOES:
                return true;
        }

        return false;
    }

    public static bool IsProps(ItemCatalogIds catalogId)
    {
        switch (catalogId)
        {
            case ItemCatalogIds.CLOTHING_HAT:
            case ItemCatalogIds.CLOTHING_GLASSES:
            case ItemCatalogIds.CLOTHING_EARS:
            case ItemCatalogIds.CLOTHING_WATCH:
            case ItemCatalogIds.CLOTHING_BRACELET:
                return true;
        }

        return false;
    }

    public static byte GetComponentId(ItemCatalogIds catalogId)
    {
        byte componentId;

        switch (catalogId)
        {
            case ItemCatalogIds.CLOTHING_HAT:
                componentId = 0;
                break;
            case ItemCatalogIds.CLOTHING_GLASSES:
                componentId = 1;
                break;
            case ItemCatalogIds.CLOTHING_EARS:
                componentId = 2;
                break;
            case ItemCatalogIds.CLOTHING_MASK:
                componentId = 1;
                break;
            case ItemCatalogIds.CLOTHING_TOP:
                componentId = 11;
                break;
            case ItemCatalogIds.CLOTHING_UNDERSHIRT:
                componentId = 8;
                break;
            case ItemCatalogIds.CLOTHING_ACCESSORIES:
                componentId = 7;
                break;
            case ItemCatalogIds.CLOTHING_WATCH:
                componentId = 6;
                break;
            case ItemCatalogIds.CLOTHING_BRACELET:
                componentId = 7;
                break;
            case ItemCatalogIds.CLOTHING_PANTS:
                componentId = 4;
                break;
            case ItemCatalogIds.CLOTHING_BACKPACK:
                componentId = 5;
                break;
            case ItemCatalogIds.CLOTHING_BODY_ARMOR:
                componentId = 9;
                break;
            case ItemCatalogIds.CLOTHING_SHOES:
                componentId = 6;
                break;
            default:
                throw new NoClothingItemException();
        }

        return componentId;
    }

    public void UpdateClothes(ServerPlayer player)
    {
        player.ClearProps(0);
        player.ClearProps(1);
        player.ClearProps(2);
        player.ClearProps(6);
        player.ClearProps(7);
        
        player.SetClothes(1, 0, 0, 2);
        player.SetClothes(3, (ushort)player.CharacterModel.Torso, (byte)player.CharacterModel.TorsoTexture, 2);
        var pantslessDrawableId = player.CharacterModel.Gender == GenderType.MALE ? 61 : 17;
        player.SetClothes(4, (ushort)pantslessDrawableId, 0, 2);
        player.SetClothes(5, 0, 0, 2);
        var shoelessDrawableId = player.CharacterModel.Gender == GenderType.MALE ? 34 : 35;
        player.SetClothes(6, (ushort)shoelessDrawableId, 0, 2);
        player.SetClothes(7, 0, 0, 2);
        player.SetClothes(8, 15, 0, 2);
        player.SetClothes(9, 0, 0, 2);
        var toplessDrawableId = player.CharacterModel.Gender == GenderType.MALE ? 15 : 18;
        player.SetClothes(11, (ushort)toplessDrawableId, 0, 2);

        foreach (var item in player.CharacterModel.InventoryModel.Items.Where(i => i is ItemClothModel 
                                                                                   && i.ItemState != ItemState.NOT_EQUIPPED)
                     .ToList()
                     .ConvertAll(i => (ItemClothModel)i))
        {
            if (IsClothes(item.CatalogItemModelId))
            {
                player.SetClothes(GetComponentId(item.CatalogItemModelId), item.DrawableId, item.TextureId, 0);
            }
            else if (IsProps(item.CatalogItemModelId))
            {
                player.SetProps(GetComponentId(item.CatalogItemModelId), item.DrawableId, item.TextureId);
            }
        }
    }

    public ClothingsData GetClothingsData(InventoryModel inventory)
    {
        var clothingsData = new ClothingsData(); 
        
        foreach (var item in inventory.Items
                     .Where(i => i is ItemClothModel && i.ItemState != ItemState.NOT_EQUIPPED)
                     .ToList()
                     .ConvertAll(i => (ItemClothModel)i))
        {

            switch (item.CatalogItemModelId)
            {
                case ItemCatalogIds.CLOTHING_HAT:
                    clothingsData.Hat = new ClothingData() { DrawableId = item.DrawableId, TextureId = item.TextureId, Title = item.Title, };
                    break;
                case ItemCatalogIds.CLOTHING_GLASSES:
                    clothingsData.Glasses = new ClothingData() { DrawableId = item.DrawableId, TextureId = item.TextureId, Title = item.Title, };
                    break;
                case ItemCatalogIds.CLOTHING_EARS:
                    clothingsData.Ears = new ClothingData() { DrawableId = item.DrawableId, TextureId = item.TextureId, Title = item.Title, };
                    break;
                case ItemCatalogIds.CLOTHING_MASK:
                    clothingsData.Mask = new ClothingData() { DrawableId = item.DrawableId, TextureId = item.TextureId, Title = item.Title, };
                    break;
                case ItemCatalogIds.CLOTHING_TOP:
                    clothingsData.Top = new ClothingData() { DrawableId = item.DrawableId, TextureId = item.TextureId, Title = item.Title, };
                    break;
                case ItemCatalogIds.CLOTHING_UNDERSHIRT:
                    clothingsData.UnderShirt = new ClothingData() { DrawableId = item.DrawableId, TextureId = item.TextureId, Title = item.Title, };
                    break;
                case ItemCatalogIds.CLOTHING_ACCESSORIES:
                    clothingsData.Accessories = new ClothingData() { DrawableId = item.DrawableId, TextureId = item.TextureId, Title = item.Title, };
                    break;
                case ItemCatalogIds.CLOTHING_WATCH:
                    clothingsData.Watch = new ClothingData() { DrawableId = item.DrawableId, TextureId = item.TextureId, Title = item.Title, };
                    break;
                case ItemCatalogIds.CLOTHING_BRACELET:
                    clothingsData.Bracelets = new ClothingData() { DrawableId = item.DrawableId, TextureId = item.TextureId, Title = item.Title, };
                    break;
                case ItemCatalogIds.CLOTHING_PANTS:
                    clothingsData.Pants = new ClothingData() { DrawableId = item.DrawableId, TextureId = item.TextureId, Title = item.Title, };
                    break;
                case ItemCatalogIds.CLOTHING_BACKPACK:
                    clothingsData.BackPack = new ClothingData() { DrawableId = item.DrawableId, TextureId = item.TextureId, Title = item.Title, };
                    break;
                case ItemCatalogIds.CLOTHING_BODY_ARMOR:
                    clothingsData.BodyArmor = new ClothingData() { DrawableId = item.DrawableId, TextureId = item.TextureId, Title = item.Title, };
                    break;
                case ItemCatalogIds.CLOTHING_SHOES:
                    clothingsData.Shoes = new ClothingData() { DrawableId = item.DrawableId, TextureId = item.TextureId, Title = item.Title, };
                    break;
                default:
                    throw new NoClothingItemException();
            }
        }

        return clothingsData;
    }
}