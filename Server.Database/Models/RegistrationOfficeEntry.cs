using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using AltV.Net;
using Server.Database.Models._Base;
using Server.Database.Models.Character;

namespace Server.Database.Models;

public class RegistrationOfficeEntry
    : ModelBase, IWritable
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int CharacterModelId { get; set; }

    public CharacterModel CharacterModel { get; set; }

    public void OnWrite(IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("lastUsage");
        writer.Value(JsonSerializer.Serialize(LastUsage));

        writer.Name("createdAt");
        writer.Value(JsonSerializer.Serialize(CreatedAt));

        writer.EndObject();
    }
}