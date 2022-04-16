using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;
using AltV.Net;
using Server.Database.Enums;
using Server.Database.Models._Base;

namespace Server.Database.Models.Inventory;

public class ItemModel
    : PositionRotationDimensionModelBase, IWritable
{
    public ItemModel()
    {
    }

    public ItemModel(ItemCatalogIds itemModelCatalogIds, int? slot, string customData, string note, int amount,
                int? condition, bool isBought, bool isStolen, ItemState itemState)
    {
        CatalogItemModelId = itemModelCatalogIds;
        Slot = slot;
        CustomData = customData;
        Note = note;
        Amount = amount;
        Condition = condition;
        IsBought = isBought;
        IsStolen = isStolen;
        ItemState = itemState;
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    public int? InventoryModelId { get; set; }
    public InventoryModel InventoryModel { get; set; }

    [JsonPropertyName("catalogItemName")] 
    public ItemCatalogIds CatalogItemModelId { get; set; }
    public CatalogItemModel CatalogItemModel { get; set; }

    public ItemType ItemType { get; set; }
    

    public int? Slot { get; set; }
    public string? DroppedByCharacter { get; set; }
    public string? CustomData { get; set; }
    public string? Note { get; set; }
    public int Amount { get; set; }
    public int? Condition { get; set; }
    public bool IsBought { get; set; }
    public bool IsStolen { get; set; }
    public ItemState ItemState { get; set; }

    public virtual void OnWrite(IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("id");
        writer.Value(Id);

        writer.Name("catalogItemName");
        writer.Value(CatalogItemModelId.ToString());

        writer.Name("catalogItem");

        writer.BeginObject();

        writer.Name("id");
        writer.Value((int)CatalogItemModel.Id);

        writer.Name("name");
        writer.Value(CatalogItemModel.Name);

        writer.Name("image");
        writer.Value(CatalogItemModel.Image);

        writer.Name("description");
        writer.Value(CatalogItemModel.Description);

        writer.Name("rarity");
        writer.Value((int)CatalogItemModel.Rarity);

        writer.Name("weight");
        writer.Value(CatalogItemModel.Weight);

        writer.Name("equippable");
        writer.Value(CatalogItemModel.Equippable);

        writer.Name("stackable");
        writer.Value(CatalogItemModel.Stackable);

        writer.Name("buyable");
        writer.Value(CatalogItemModel.Buyable);

        writer.Name("sellable");
        writer.Value(CatalogItemModel.Sellable);

        writer.Name("price");
        writer.Value(CatalogItemModel.Price);

        writer.Name("sellPrice");
        writer.Value(CatalogItemModel.SellPrice);

        writer.EndObject();

        writer.Name("slot");
        writer.Value(Slot ?? -1);

        writer.Name("droppedByCharacter");
        writer.Value(DroppedByCharacter ?? "Unbekannt");

        writer.Name("customData");
        writer.Value(CustomData);

        writer.Name("note");
        writer.Value(Note);

        writer.Name("amount");
        writer.Value(Amount);

        writer.Name("condition");
        writer.Value(Condition ?? -1);

        writer.Name("isBought");
        writer.Value(IsBought);

        writer.Name("itemState");
        writer.Value((int)ItemState);

        writer.Name("positionX");
        writer.Value(PositionX);

        writer.Name("positionY");
        writer.Value(PositionY);

        writer.Name("positionZ");
        writer.Value(PositionZ);

        writer.Name("lastUsage");
        writer.Value(JsonSerializer.Serialize(LastUsage));

        writer.EndObject();
    }
}