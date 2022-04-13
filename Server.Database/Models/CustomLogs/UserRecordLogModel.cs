using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using AltV.Net;
using Server.Database.Enums;
using Server.Database.Models._Base;
using Server.Database.Models.Character;

namespace Server.Database.Models.CustomLogs;

public class UserRecordLogModel
    : ModelBase, IWritable
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    public ulong AccountModelId { get; set; }
    public AccountModel? AccountModel { get; set; }

    public ulong StaffAccountModelId { get; set; }
    public AccountModel? StaffAccountModel { get; set; }

    public int? CharacterModelId { get; set; }
    public CharacterModel? CharacterModel { get; set; }

    public UserRecordType UserRecordType { get; set; }

    [MaxLength(2048)] public string Text { get; set; }

    public DateTime LoggedAt { get; set; }

    public void OnWrite(IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("id");
        writer.Value(Id);

        writer.Name("accountId");
        writer.Value(AccountModelId);

        writer.Name("accountName");
        writer.Value(AccountModel.CurrentName);

        writer.Name("staffAccountId");
        writer.Value(StaffAccountModelId);

        writer.Name("staffAccountName");
        writer.Value(StaffAccountModel.CurrentName);

        writer.Name("characterId");
        writer.Value(CharacterModelId ?? -1);

        writer.Name("characterName");
        writer.Value(CharacterModelId != null ? CharacterModel.Name : "Ohne Charakter");

        writer.Name("userRecordType");
        writer.Value((int)UserRecordType);

        writer.Name("text");
        writer.Value(Text);

        writer.Name("loggedAt");
        writer.Value(JsonSerializer.Serialize(LoggedAt));
        ;

        writer.EndObject();
    }
}