using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using AltV.Net;
using Server.Database.Models._Base;
using Server.Database.Models.Character;

namespace Server.Database.Models;

public class RoleplayInfoModel
    : PositionRotationModelBase, IWritable
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
        writer.BeginObject();

        writer.Name("id");
        writer.Value(Id);

        writer.Name("characterName");
        writer.Value(CharacterModel.Name);

        writer.Name("context");
        writer.Value(Context);

        writer.Name("dimension");
        writer.Value(Dimension);

        writer.Name("distance");
        writer.Value(Distance);

        writer.Name("positionX");
        writer.Value(PositionX);

        writer.Name("positionY");
        writer.Value(PositionY);

        writer.Name("positionZ");
        writer.Value(PositionZ);

        writer.Name("pitch");
        writer.Value(Pitch);

        writer.Name("roll");
        writer.Value(Roll);

        writer.Name("yaw");
        writer.Value(Yaw);

        writer.Name("createdAtJson");
        writer.Value(JsonSerializer.Serialize(CreatedAt));

        writer.EndObject();
    }
}