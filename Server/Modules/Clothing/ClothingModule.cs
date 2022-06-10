using Server.Core.Abstractions.ScriptStrategy;
using Server.Database.Enums;

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

    public static int? GetComponentId(ItemCatalogIds catalogId)
    {
        int? componentId = null;

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
        }

        return componentId;
    }
}