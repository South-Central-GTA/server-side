using Server.Database.Enums;

namespace Server.Database.Models.Inventory;

public class ItemRadioModel
    : ItemModel
{
    public FactionType FactionType { get; set; }
    public int Frequency { get; set; }
}