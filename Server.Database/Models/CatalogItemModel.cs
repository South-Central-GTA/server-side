﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using AltV.Net;
using AltV.Net.Data;
using Server.Database.Enums;
using Server.Database.Models._Base;

namespace Server.Database.Models;

public class CatalogItemModel : PositionRotationModelBase, IWritable
{
    public CatalogItemModel()
    {
    }

    public CatalogItemModel(ItemCatalogIds id, string name, string model, Rotation rotation, float zOffset,
        string image, string description, Rarity rarity, float weight, bool equippable, bool stackable, bool buyable,
        bool sellable, int price, int sellPrice, int? maxLimit = null)
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
        Rarity = rarity;
        Weight = weight;
        Equippable = equippable;
        Stackable = stackable;
        Buyable = buyable;
        Sellable = sellable;
        Price = price;
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
    public Rarity Rarity { get; set; }
    public float Weight { get; set; }
    public bool Equippable { get; set; }
    public bool Stackable { get; set; }
    public bool Buyable { get; set; }
    public bool Sellable { get; set; }
    public int Price { get; set; }
    public int? MaxLimit { get; set; }

    public void OnWrite(IMValueWriter writer)
    {
        Serialize(this, writer);
    }

    public static void Serialize(CatalogItemModel model, IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("id");
        writer.Value((int)model.Id);

        writer.Name("name");
        writer.Value(model.Name);

        writer.Name("image");
        writer.Value(model.Image);

        writer.Name("description");
        writer.Value(model.Description);

        writer.Name("rarity");
        writer.Value((int)model.Rarity);

        writer.Name("weight");
        writer.Value(model.Weight);

        writer.Name("equippable");
        writer.Value(model.Equippable);

        writer.Name("stackable");
        writer.Value(model.Stackable);

        writer.Name("buyable");
        writer.Value(model.Buyable);

        writer.Name("sellable");
        writer.Value(model.Sellable);

        writer.Name("price");
        writer.Value(model.Price);

        writer.Name("lastUsageJson");
        writer.Value(JsonSerializer.Serialize(model.LastUsage));

        writer.EndObject();
    }
}