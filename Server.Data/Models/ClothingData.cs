using System.Text.Json.Serialization;
using AltV.Net;
using Server.Database.Enums;

namespace Server.Data.Models;

public class ClothingData
{
    [JsonPropertyName("genderType")]
    public GenderType GenderType { get; set; }

    [JsonPropertyName("drawableId")]
    public byte DrawableId { get; set; }

    [JsonPropertyName("textureId")]
    public byte TextureId { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    public static void Serialize(ClothingData data, IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("genderType");
        writer.Value((int)data.GenderType);

        writer.Name("drawableId");
        writer.Value(data.DrawableId);

        writer.Name("textureId");
        writer.Value(data.TextureId);

        writer.Name("title");
        writer.Value(data.Title);

        writer.EndObject();
    }
}