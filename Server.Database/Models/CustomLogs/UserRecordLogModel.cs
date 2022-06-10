using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using AltV.Net;
using Server.Database.Enums;
using Server.Database.Models._Base;
using Server.Database.Models.Character;

namespace Server.Database.Models.CustomLogs;

public class UserRecordLogModel : ModelBase, IWritable
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

    public void OnWrite(IMValueWriter writer)
    {
        Serialize(this, writer);
    }

    public static void Serialize(UserRecordLogModel model, IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("id");
        writer.Value(model.Id);

        writer.Name("accountId");
        writer.Value(model.AccountModelId);

        writer.Name("accountName");
        writer.Value(model.AccountModel != null ? model.AccountModel.CurrentName : string.Empty);

        writer.Name("staffAccountId");
        writer.Value(model.StaffAccountModelId);

        writer.Name("staffAccountName");
        writer.Value(model.StaffAccountModel != null ? model.StaffAccountModel.CurrentName : string.Empty);

        writer.Name("characterId");
        writer.Value(model.CharacterModelId ?? -1);

        writer.Name("characterName");
        writer.Value(model.CharacterModel != null ? model.CharacterModel.Name : "Ohne Charakter");

        writer.Name("userRecordType");
        writer.Value((int)model.UserRecordType);

        writer.Name("text");
        writer.Value(model.Text);

        writer.Name("createdAtJson");
        writer.Value(JsonSerializer.Serialize(model.CreatedAt));

        writer.EndObject();
    }
}