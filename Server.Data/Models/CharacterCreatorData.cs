using System.Collections.Generic;
using System.Text.Json.Serialization;
using Server.Database.Models.Character;

namespace Server.Data.Models;

public class CharacterCreatorData
{
    [JsonPropertyName("character")]
    public CharacterModel CharacterModel { get; set; }
    
    [JsonPropertyName("clothes")]
    public ClothingsData ClothingsData { get; set; }

    [JsonPropertyName("startMoney")]
    public int StartMoney { get; set; }

    [JsonPropertyName("hasPhone")]
    public bool HasPhone { get; set; }

    [JsonPropertyName("isRegistered")]
    public bool IsRegistered { get; set; }

    [JsonPropertyName("hasDrivingLicense")]
    public bool HasDrivingLicense { get; set; }

    [JsonPropertyName("purchaseOrders")]
    public List<CharacterCreatorPurchaseOrder> PurchaseOrders { get; set; }

    [JsonPropertyName("spawnId")]
    public int SpawnId { get; set; }
}