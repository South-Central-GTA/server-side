using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using AltV.Net;
using Server.Database.Enums;
using Server.Database.Models._Base;
using Server.Database.Models.Character;

namespace Server.Database.Models.CustomLogs;

public class CommandLogModel : ModelBase, IWritable
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    public ulong AccountModelId { get; set; }
    public AccountModel? AccountModel { get; set; }

    public int CharacterModelId { get; set; }
    public CharacterModel? CharacterModel { get; set; }

    [MaxLength(2048)] public string Name { get; set; }

    [MaxLength(2048)] public string Arguments { get; set; }

    public Permission RequiredPermission { get; set; }

    public void OnWrite(IMValueWriter writer)
    {
        Serialize(this, writer);
    }

    public static void Serialize(CommandLogModel model, IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("name");
        writer.Value(model.Name);

        writer.Name("arguments");
        writer.Value(model.Arguments);

        writer.Name("accountName");
        writer.Value(model.AccountModel.CurrentName);

        writer.Name("createdAtJson");
        writer.Value(JsonSerializer.Serialize(model.CreatedAt));

        writer.EndObject();
    }
}