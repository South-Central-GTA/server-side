using System.Text.Json.Serialization;
using Server.Database.Enums;

namespace Server.Data.Models;

public class ClothingData
{
    [JsonPropertyName("genderType")] public GenderType GenderType { get; set; }

    [JsonPropertyName("drawableId")] public int DrawableId { get; set; }

    [JsonPropertyName("textureId")] public int TextureId { get; set; }

    [JsonPropertyName("title")] public string Title { get; set; }
}