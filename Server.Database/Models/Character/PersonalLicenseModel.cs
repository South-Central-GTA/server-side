using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Server.Database.Enums;
using Server.Database.Models._Base;

namespace Server.Database.Models.Character;

public class PersonalLicenseModel : ModelBase
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    public int CharacterModelId { get; set; }

    public CharacterModel CharacterModel { get; set; }


    public PersonalLicensesType Type { get; set; }
    public int Warnings { get; set; }
}