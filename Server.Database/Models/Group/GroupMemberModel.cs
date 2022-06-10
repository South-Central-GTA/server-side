using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AltV.Net;
using Server.Database.Models._Base;
using Server.Database.Models.Character;

namespace Server.Database.Models.Group;

public class GroupMemberModel : ModelBase, IWritable
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int GroupModelId { get; set; }

    public GroupModel GroupModel { get; set; }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int CharacterModelId { get; set; }

    public CharacterModel? CharacterModel { get; set; }

    public uint RankLevel { get; set; }

    public uint Salary { get; set; }
    public int BankAccountId { get; set; }
    public bool Owner { get; set; }

    public void OnWrite(IMValueWriter writer)
    {
        Serialize(this, writer);
    }

    public static void Serialize(GroupMemberModel model, IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("groupId");
        writer.Value(model.GroupModelId);

        writer.Name("characterId");
        writer.Value(model.CharacterModelId);

        writer.Name("characterName");
        writer.Value(model.CharacterModel?.Name ?? string.Empty);

        writer.Name("level");
        writer.Value(model.RankLevel);

        writer.Name("salary");
        writer.Value(model.Salary);

        writer.Name("bankAccountId");
        writer.Value(model.BankAccountId);

        writer.Name("owner");
        writer.Value(model.Owner);

        writer.EndObject();
    }
}