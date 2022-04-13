namespace Server.Database.Models.Inventory;

public class ItemKeyModel
    : ItemModel
{
    public int? HouseModelId { get; set; }
    public int? PlayerVehicleModelId { get; set; }
}