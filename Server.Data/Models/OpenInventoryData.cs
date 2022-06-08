using Server.Database.Enums;

namespace Server.Data.Models;

public class OpenInventoryData
{
    public OpenInventoryData(InventoryType inventoryType, int inventoryId)
    {
        InventoryType = inventoryType;
        InventoryId = inventoryId;
    }

    public InventoryType InventoryType { get; }
    public int InventoryId { get; }
}