using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using AltV.Net;
using Server.Database.Enums;
using Server.Database.Models._Base;
using Server.Database.Models.Character;

namespace Server.Database.Models.CustomLogs;

public class CommandLogModel
    : ModelBase, IWritable
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

    public DateTime LoggedAt { get; set; }

    public Permission RequiredPermission { get; set; }

    public void OnWrite(IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("name");
        writer.Value(Name);

        writer.Name("arguments");
        writer.Value(Arguments);

        writer.Name("accountName");
        writer.Value(AccountModel.CurrentName);

        writer.Name("loggedAtJson");
        writer.Value(JsonSerializer.Serialize(LoggedAt));

        writer.EndObject();
    }
}