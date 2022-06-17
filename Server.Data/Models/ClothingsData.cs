using System.Text.Json.Serialization;
using AltV.Net;

namespace Server.Data.Models;

public class ClothingsData
{
    [JsonPropertyName("hat")]
    public ClothingData? Hat { get; set; } = null;

    [JsonPropertyName("glasses")]
    public ClothingData? Glasses { get; set; } = null;

    [JsonPropertyName("ears")]
    public ClothingData? Ears { get; set; } = null;

    [JsonPropertyName("watch")]
    public ClothingData? Watch { get; set; } = null;

    [JsonPropertyName("bracelets")]
    public ClothingData? Bracelets { get; set; } = null;

    [JsonPropertyName("mask")]
    public ClothingData? Mask { get; set; } = null;

    [JsonPropertyName("top")]
    public ClothingData? Top { get; set; } = null;

    [JsonPropertyName("bodyArmor")]
    public ClothingData? BodyArmor { get; set; } = null;

    [JsonPropertyName("backPack")]
    public ClothingData? BackPack { get; set; } = null;

    [JsonPropertyName("underShirt")]
    public ClothingData? UnderShirt { get; set; } = null;

    [JsonPropertyName("accessories")]
    public ClothingData? Accessories { get; set; } = null;

    [JsonPropertyName("pants")]
    public ClothingData? Pants { get; set; } = null;

    [JsonPropertyName("shoes")]
    public ClothingData? Shoes { get; set; } = null;

    public static void Serialize(ClothingsData data, IMValueWriter writer)
    {
        writer.BeginObject();

        if (data.Hat != null)
        {
            writer.Name("hat");
            ClothingData.Serialize(data.Hat, writer);
        }
        if (data.Glasses != null)
        {
            writer.Name("glasses");
            ClothingData.Serialize(data.Glasses, writer);
        }
        if (data.Ears != null)
        {
            writer.Name("ears");
            ClothingData.Serialize(data.Ears, writer);
        }
        if (data.Watch != null)
        {
            writer.Name("watch");
            ClothingData.Serialize(data.Watch, writer);
        }
        if (data.Bracelets != null)
        {
            writer.Name("bracelets");
            ClothingData.Serialize(data.Bracelets, writer);
        }
        if (data.Mask != null)
        {
            writer.Name("mask");
            ClothingData.Serialize(data.Mask, writer);
        }
        if (data.Top != null)
        {
            writer.Name("top");
            ClothingData.Serialize(data.Top, writer);
        }
        if (data.BodyArmor != null)
        {
            writer.Name("bodyArmor");
            ClothingData.Serialize(data.BodyArmor, writer);
        }
        if (data.BackPack != null)
        {
            writer.Name("backPack");
            ClothingData.Serialize(data.BackPack, writer);
        }
        if (data.UnderShirt != null)
        {
            writer.Name("underShirt");
            ClothingData.Serialize(data.UnderShirt, writer);
        }
        if (data.Accessories != null)
        {
            writer.Name("accessories");
            ClothingData.Serialize(data.Accessories, writer);
        }
        if (data.Pants != null)
        {
            writer.Name("pants");
            ClothingData.Serialize(data.Pants, writer);
        }
        if (data.Shoes != null)
        {
            writer.Name("shoes");
            ClothingData.Serialize(data.Shoes, writer);
        }

        writer.EndObject();
    }
}