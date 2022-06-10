using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using AltV.Net;
using Server.Database.Models._Base;
using Server.Database.Models.Character;

namespace Server.Database.Models.Mdc;

public class CriminalRecordModel : ModelBase, IWritable
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    public int CharacterModelId { get; set; }
    public CharacterModel CharacterModel { get; set; }

    public string CreatorCharacterName { get; set; }

    public string Reason { get; set; }

    public void OnWrite(IMValueWriter writer)
    {
        Serialize(this, writer);
    }

    public static void Serialize(CriminalRecordModel model, IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("id");
        writer.Value(model.Id);

        writer.Name("reason");
        writer.Value(model.Reason);

        writer.Name("creatorCharacterName");
        writer.Value(model.CreatorCharacterName);

        writer.Name("createdAtJson");
        writer.Value(JsonSerializer.Serialize(model.CreatedAt));

        writer.EndObject();
    }
}