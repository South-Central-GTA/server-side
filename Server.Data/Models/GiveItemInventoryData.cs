using System.Text.Json.Serialization;
using Server.Database.Enums;

namespace Server.Data.Models;

public class GiveItemInventoryData
{
    [JsonPropertyName("itemId")] public int ItemId { get; set; }

    [JsonPropertyName("invType")] public InventoryType InvType { get; set; }

    [JsonPropertyName("characterId")] public int CharacterId { get; set; }
}