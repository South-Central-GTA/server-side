using Server.Database.Enums;

namespace Server.Data.Models;

public class ItemTransferData
{
    public InventoryType OldInvType { get; set; }
    public InventoryType TargetInvType { get; set; }
    public int ItemId { get; set; }
    public int Slot { get; set; }
}