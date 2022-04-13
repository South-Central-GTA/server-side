using System.Collections.Generic;
using Server.Database.Enums;

namespace Server.Data.Models;

public class UnboughtItemsData
{
    public bool GotWarned { get; set; }
    public Dictionary<ItemCatalogIds, int> AmountOfItemsUnbought { get; set; }
}