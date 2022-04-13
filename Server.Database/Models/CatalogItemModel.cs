using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AltV.Net;
using AltV.Net.Data;
using Server.Database.Enums;
using Server.Database.Models._Base;

namespace Server.Database.Models;

public class CatalogItemModel
    : PositionRotationModelBase, IWritable
{
    public CatalogItemModel()
    {
    }

    public CatalogItemModel(ItemCatalogIds id, string name, string model, Rotation rotation, float zOffset, string image, string description, int useValue, Rarity rarity, float weight, bool equippable,
                       bool stackable, bool buyable, bool sellable, int price, int sellPrice, int? maxLimit = null)
    {
        Id = id;
        Name = name;
        Model = model;
        Pitch = rotation.Pitch;
        Roll = rotation.Roll;
        Yaw = rotation.Yaw;
        ZOffset = zOffset;
        Image = image;
        Description = description;
        UseValue = useValue;
        Rarity = rarity;
        Weight = weight;
        Equippable = equippable;
        Stackable = stackable;
        Buyable = buyable;
        Sellable = sellable;
        Price = price;
        SellPrice = sellPrice;
        MaxLimit = maxLimit;
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public ItemCatalogIds Id { get; set; }

    public string Name { get; set; }
    public string Model { get; set; }
    public float ZOffset { get; set; }
    public string Image { get; set; }
    public string Description { get; set; }
    public int UseValue { get; set; }
    public Rarity Rarity { get; set; }
    public float Weight { get; set; }
    public bool Equippable { get; set; }
    public bool Stackable { get; set; }
    public bool Buyable { get; set; }
    public bool Sellable { get; set; }
    public int Price { get; set; }
    public int SellPrice { get; set; }
    public int? MaxLimit { get; set; }

    public void OnWrite(IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("id");
        writer.Value((int)Id);

        writer.Name("name");
        writer.Value(Name);

        writer.Name("image");
        writer.Value(Image);

        writer.Name("description");
        writer.Value(Description);

        writer.Name("useValue");
        writer.Value(UseValue);

        writer.Name("rarity");
        writer.Value((int)Rarity);

        writer.Name("weight");
        writer.Value(Weight);

        writer.Name("equippable");
        writer.Value(Equippable);

        writer.Name("stackable");
        writer.Value(Stackable);

        writer.Name("buyable");
        writer.Value(Buyable);

        writer.Name("sellable");
        writer.Value(Sellable);

        writer.Name("price");
        writer.Value(Price);

        writer.Name("sellPrice");
        writer.Value(SellPrice);

        writer.EndObject();
    }
}