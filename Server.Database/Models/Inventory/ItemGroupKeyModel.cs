using Server.Database.Models.Group;

namespace Server.Database.Models.Inventory;

public class ItemGroupKeyModel
    : ItemModel
{
    public int GroupModelId { get; set; }
    public GroupModel GroupModel { get; set; }
}