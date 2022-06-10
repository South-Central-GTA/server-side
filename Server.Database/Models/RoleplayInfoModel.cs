using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using AltV.Net;
using Server.Database.Models._Base;
using Server.Database.Models.Character;

namespace Server.Database.Models;

public class RoleplayInfoModel : PositionRotationModelBase, IWritable
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    public ulong MarkerId { get; set; }

    public int CharacterModelId { get; set; }
    public CharacterModel CharacterModel { get; set; }

    public int Dimension { get; set; }
    public int Distance { get; set; }

    public string Context { get; set; }

    public virtual void OnWrite(IMValueWriter writer)
    {
        Serialize(this, writer);
    }

    public static void Serialize(RoleplayInfoModel model, IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("id");
        writer.Value(model.Id);

        writer.Name("characterName");
        writer.Value(model.CharacterModel.Name);

        writer.Name("context");
        writer.Value(model.Context);

        writer.Name("dimension");
        writer.Value(model.Dimension);

        writer.Name("distance");
        writer.Value(model.Distance);

        writer.Name("positionX");
        writer.Value(model.PositionX);

        writer.Name("positionY");
        writer.Value(model.PositionY);

        writer.Name("positionZ");
        writer.Value(model.PositionZ);

        writer.Name("pitch");
        writer.Value(model.Pitch);

        writer.Name("roll");
        writer.Value(model.Roll);

        writer.Name("yaw");
        writer.Value(model.Yaw);

        writer.Name("createdAtJson");
        writer.Value(JsonSerializer.Serialize(model.CreatedAt));

        writer.EndObject();
    }
}