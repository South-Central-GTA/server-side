using System.Text.Json.Serialization;
using Server.Data.Enums;
using Server.Database.Models.Vehicles;

namespace Server.Data.Models;

public class CharacterCreatorPurchaseOrder
{
    [JsonPropertyName("id")] public int Id { get; set; }

    [JsonPropertyName("type")] public CharacterCreatorPurchaseType Type { get; set; }

    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("description")] public string Description { get; set; }

    [JsonPropertyName("southCentralPoints")]
    public int SouthCentralPoints { get; set; }

    [JsonPropertyName("removeable")] public bool Removeable { get; set; }

    [JsonPropertyName("orderedVehicle")] public CatalogVehicleModel? OrderedVehicle { get; set; }
}