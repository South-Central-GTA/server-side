using System.Collections.Generic;

namespace Server.Database.Models.Inventory;

public class ItemWeaponModel
    : ItemModel
{
    public string? SerialNumber { get; set; }
    public int InitialOwnerId { get; set; }
    public List<string> ComponentHashes { get; init; } = new();
}