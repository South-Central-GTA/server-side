using Server.Core.Abstractions.ScriptStrategy;
using Server.Database.Enums;

namespace Server.Modules.Consumeable;

public class ConsumeableItemModule
    : ITransientScript
{
    public static bool IsConsumeable(ItemCatalogIds catalogId)
    {
        switch (catalogId)
        {
            case ItemCatalogIds.ALC_DRINKS:
            case ItemCatalogIds.NON_ALC_DRINKS:
            case ItemCatalogIds.FAST_FOOD:
            case ItemCatalogIds.HEALTHY_FOOD:
            case ItemCatalogIds.BREAD:
            case ItemCatalogIds.SANDWICH:
            case ItemCatalogIds.SOUP:
            case ItemCatalogIds.MEAT:
            case ItemCatalogIds.SWEETS:
            case ItemCatalogIds.CANDY:
            case ItemCatalogIds.DRUG_MARIJUANA:
            case ItemCatalogIds.DRUG_COCAINE: 
            case ItemCatalogIds.DRUG_MDMA:
            case ItemCatalogIds.DRUG_XANAX:
            case ItemCatalogIds.DRUG_CODEINE:
            case ItemCatalogIds.DRUG_METH:
                return true;
        }

        return false;
    }
}