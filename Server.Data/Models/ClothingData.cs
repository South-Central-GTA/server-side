using System.Text.Json.Serialization;

namespace Server.Data.Models;

public class ClothingsData
{
    [JsonPropertyName("hat")] public ClothingData? Hat { get; set; } = null;

    [JsonPropertyName("glasses")] public ClothingData? Glasses { get; set; } = null;

    [JsonPropertyName("ears")] public ClothingData? Ears { get; set; } = null;

    [JsonPropertyName("watch")] public ClothingData? Watch { get; set; } = null;

    [JsonPropertyName("bracelets")] public ClothingData? Bracelets { get; set; } = null;

    [JsonPropertyName("mask")] public ClothingData? Mask { get; set; } = null;

    [JsonPropertyName("top")] public ClothingData? Top { get; set; } = null;

    [JsonPropertyName("bodyArmor")] public ClothingData? BodyArmor { get; set; } = null;

    [JsonPropertyName("backPack")] public ClothingData? BackPack { get; set; } = null;

    [JsonPropertyName("underShirt")] public ClothingData? UnderShirt { get; set; } = null;

    [JsonPropertyName("accessories")] public ClothingData? Accessories { get; set; } = null;

    [JsonPropertyName("pants")] public ClothingData? Pants { get; set; } = null;

    [JsonPropertyName("shoes")] public ClothingData? Shoes { get; set; } = null;
}