using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AltV.Net;
using Server.Database.Enums;
using Server.Database.Models._Base;
using Server.Database.Models.Character;
using Server.Database.Models.Housing;
using Server.Database.Models.Vehicles;

namespace Server.Database.Models.Inventory;

public class InventoryModel
    : ModelBase, IWritable
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    public int? CharacterModelId { get; set; }
    public CharacterModel? CharacterModel { get; set; }

    public int? HouseModelId { get; set; }
    public HouseModel? HouseModel { get; set; }

    public int? VehicleModelId { get; set; }
    public PlayerVehicleModel? VehicleModel { get; set; }

    public int? ItemClothModelId { get; set; }
    public ItemClothModel? ItemClothModel { get; set; }

    public int? GroupCharacterId { get; set; }
    public int? GroupId { get; set; }

    public InventoryType InventoryType { get; set; }

    public List<ItemModel> Items { get; init; } = new();

    public float MaxWeight { get; set; }
    public string Name { get; set; }

    public void OnWrite(IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("id");
        writer.Value(Id);

        writer.Name("name");
        writer.Value(Name);

        writer.Name("inventoryType");
        writer.Value((int)InventoryType);

        writer.Name("items");

        writer.BeginArray();

        foreach (var item in Items)
        {
            writer.BeginObject();

            writer.Name("id");
            writer.Value(item.Id);

            writer.Name("catalogItemName");
            writer.Value(item.CatalogItemModelId.ToString());

            writer.Name("catalogItem");

            writer.BeginObject();

            writer.Name("id");
            writer.Value((int)item.CatalogItemModel.Id);

            writer.Name("name");
            writer.Value(item.CatalogItemModel.Name);

            writer.Name("image");
            writer.Value(item.CatalogItemModel.Image);

            writer.Name("description");
            writer.Value(item.CatalogItemModel.Description);

            writer.Name("useValue");
            writer.Value(item.CatalogItemModel.UseValue);

            writer.Name("rarity");
            writer.Value((int)item.CatalogItemModel.Rarity);

            writer.Name("weight");
            writer.Value(item.CatalogItemModel.Weight);

            writer.Name("equippable");
            writer.Value(item.CatalogItemModel.Equippable);

            writer.Name("stackable");
            writer.Value(item.CatalogItemModel.Stackable);

            writer.Name("buyable");
            writer.Value(item.CatalogItemModel.Buyable);

            writer.Name("sellable");
            writer.Value(item.CatalogItemModel.Sellable);

            writer.Name("price");
            writer.Value(item.CatalogItemModel.Price);

            writer.Name("sellPrice");
            writer.Value(item.CatalogItemModel.SellPrice);

            writer.EndObject();

            writer.Name("slot");
            writer.Value(item.Slot ?? -1);

            writer.Name("customData");
            writer.Value(item.CustomData);

            writer.Name("note");
            writer.Value(item.Note);

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

        writer.EndArray();

        writer.Name("maxWeight");
        writer.Value(MaxWeight);

        writer.EndObject();
    }
}