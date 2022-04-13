using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Database.Models.Character;

public class FaceFeaturesModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int CharacterModelId { get; set; }

    public CharacterModel CharacterModel { get; set; }

    public float EyesSize { get; set; }
    public float LipsThickness { get; set; }
    public float NoseWidth { get; set; }
    public float NoseHeight { get; set; }
    public float NoseLength { get; set; }
    public float NoseBridge { get; set; }
    public float NoseTip { get; set; }
    public float NoseBridgeShift { get; set; }
    public float BrowHeight { get; set; }
    public float BrowWidth { get; set; }
    public float CheekboneHeight { get; set; }
    public float CheekboneWidth { get; set; }
    public float CheekWidth { get; set; }
    public float JawWidth { get; set; }
    public float JawHeight { get; set; }
    public float ChinLength { get; set; }
    public float ChinPosition { get; set; }
    public float ChinWidth { get; set; }
    public float ChinShape { get; set; }
    public float NeckWidth { get; set; }
}