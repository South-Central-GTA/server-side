using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Server.Database.Models._Base;

namespace Server.Database.Models.Character;

public class TattoosModel
    : ModelBase
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int CharacterModelId { get; set; }

    public CharacterModel CharacterModel { get; set; }

    [JsonPropertyName("headCollection")] public string HeadCollection { get; set; } = "";

    [JsonPropertyName("headHash")] public string HeadHash { get; set; } = "";

    [JsonPropertyName("torsoCollection")] public string TorsoCollection { get; set; } = "";

    [JsonPropertyName("torsoHash")] public string TorsoHash { get; set; } = "";

    [JsonPropertyName("leftArmCollection")]
    public string LeftArmCollection { get; set; } = "";

    [JsonPropertyName("leftArmHash")] public string LeftArmHash { get; set; } = "";

    [JsonPropertyName("rightArmCollection")]
    public string RightArmCollection { get; set; } = "";

    [JsonPropertyName("rightArmHash")] public string RightArmHash { get; set; } = "";

    [JsonPropertyName("leftLegCollection")]
    public string LeftLegCollection { get; set; } = "";

    [JsonPropertyName("leftLegHash")] public string LeftLegHash { get; set; } = "";

    [JsonPropertyName("rightLegCollection")]
    public string RightLegCollection { get; set; } = "";

    [JsonPropertyName("rightLegHash")] public string RightLegHash { get; set; } = "";

    public void Update(TattoosModel tattoosModel)
    {
        HeadCollection = tattoosModel.HeadCollection;
        HeadHash = tattoosModel.HeadHash;

        TorsoCollection = tattoosModel.TorsoCollection;
        TorsoHash = tattoosModel.TorsoHash;

        LeftArmCollection = tattoosModel.LeftArmCollection;
        LeftArmHash = tattoosModel.LeftArmHash;

        RightArmCollection = tattoosModel.RightArmCollection;
        RightArmHash = tattoosModel.RightArmHash;

        LeftLegCollection = tattoosModel.LeftLegCollection;
        LeftLegHash = tattoosModel.LeftLegHash;

        RightLegCollection = tattoosModel.RightLegCollection;
        RightLegHash = tattoosModel.RightLegHash;
    }

    public int Diff(TattoosModel tattoosModel)
    {
        var diffs = 0;
        if (HeadHash != tattoosModel.HeadHash)
        {
            diffs++;
        }

        if (TorsoHash != tattoosModel.TorsoHash)
        {
            diffs++;
        }

        if (LeftArmHash != tattoosModel.LeftArmHash)
        {
            diffs++;
        }

        if (RightArmHash != tattoosModel.RightArmHash)
        {
            diffs++;
        }

        if (LeftLegHash != tattoosModel.LeftLegHash)
        {
            diffs++;
        }

        if (RightLegHash != tattoosModel.RightLegHash)
        {
            diffs++;
        }

        return diffs;
    }
}