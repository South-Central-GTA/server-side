using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using AltV.Net;
using Server.Database.Enums;
using Server.Database.Models._Base;

namespace Server.Database.Models.Inventory;

public class ItemModel : PositionRotationDimensionModelBase, IWritable
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

    [JsonPropertyName("catalogItemName")] public ItemCatalogIds CatalogItemModelId { get; set; }
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
        Serialize(this, writer);
    }

    public static void Serialize(ItemModel item, IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("id");
        writer.Value(item.Id);

        writer.Name("catalogItemName");
        writer.Value(item.CatalogItemModelId.ToString());

        writer.Name("catalogItem");

        CatalogItemModel.Serialize(item.CatalogItemModel, writer);

        writer.Name("slot");
        writer.Value(item.Slot ?? -1);

        writer.Name("customData");
        writer.Value(item.CustomData ?? string.Empty);

        writer.Name("note");
        writer.Value(item.Note ?? string.Empty);

        writer.Name("amount");
        writer.Value(item.Amount);

        writer.Name("condition");
        writer.Value(item.Condition ?? -1);

        writer.Name("isBought");
        writer.Value(item.IsBought);

        writer.Name("itemState");
        writer.Value((int)item.ItemState);

        var value = -1;
        if (item is ItemWeaponAttachmentModel weaponAttachment)
        {
            value = weaponAttachment.ItemWeaponId ?? -1;
        }

        writer.Name("attachedToWeaponItem");
        writer.Value(value);

        writer.EndObject();
    }
}