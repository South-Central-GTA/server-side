using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Server.Database.Models._Base;
using Server.Database.Models.Character;

namespace Server.Database.Models.Group;

public class GroupMemberModel
    : ModelBase
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
}