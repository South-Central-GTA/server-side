using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Server.Database.Models._Base;

namespace Server.Database.Models.Character;

public class DefinedJobModel : ModelBase
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int CharacterModelId { get; set; }

    public CharacterModel CharacterModel { get; set; }

    public int JobId { get; set; }
    public int BankAccountId { get; set; }
}