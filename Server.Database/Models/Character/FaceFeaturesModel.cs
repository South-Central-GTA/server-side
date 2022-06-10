using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AltV.Net;

namespace Server.Database.Models.Character;

public class FaceFeaturesModel : IWritable
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

    public void OnWrite(IMValueWriter writer)
    {
        Serialize(this, writer);
    }

    public static void Serialize(FaceFeaturesModel model, IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("eyesSize");
        writer.Value(model.EyesSize);

        writer.Name("lipsThickness");
        writer.Value(model.LipsThickness);

        writer.Name("noseWidth");
        writer.Value(model.NoseWidth);

        writer.Name("noseHeight");
        writer.Value(model.NoseHeight);

        writer.Name("noseLength");
        writer.Value(model.NoseLength);

        writer.Name("noseBridge");
        writer.Value(model.NoseBridge);

        writer.Name("noseTip");
        writer.Value(model.NoseTip);

        writer.Name("noseBridgeShift");
        writer.Value(model.NoseBridgeShift);

        writer.Name("browHeight");
        writer.Value(model.BrowHeight);

        writer.Name("browWidth");
        writer.Value(model.BrowWidth);

        writer.Name("cheekboneHeight");
        writer.Value(model.CheekboneHeight);

        writer.Name("cheekboneWidth");
        writer.Value(model.CheekboneWidth);

        writer.Name("cheekWidth");
        writer.Value(model.CheekWidth);

        writer.Name("jawWidth");
        writer.Value(model.JawWidth);

        writer.Name("jawHeight");
        writer.Value(model.JawHeight);

        writer.Name("chinLength");
        writer.Value(model.ChinLength);

        writer.Name("chinPosition");
        writer.Value(model.ChinPosition);

        writer.Name("chinWidth");
        writer.Value(model.ChinWidth);

        writer.Name("chinShape");
        writer.Value(model.ChinShape);

        writer.Name("neckWidth");
        writer.Value(model.NeckWidth);

        writer.EndObject();
    }
}