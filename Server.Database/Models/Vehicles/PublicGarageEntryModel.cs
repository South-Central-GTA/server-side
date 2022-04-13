using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Server.Database.Models._Base;
using Server.Database.Models.Character;
using Server.Database.Models.Group;

namespace Server.Database.Models.Vehicles;

public class PublicGarageEntryModel
    : ModelBase
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    public int? GroupModelId { get; set; }
    public GroupModel? GroupModel { get; set; }

    public int? CharacterModelId { get; set; }
    public CharacterModel? CharacterModel { get; set; }

    public int PlayerVehicleModelId { get; set; }
    public PlayerVehicleModel PlayerVehicleModel { get; set; }

    public int GarageId { get; set; }

    public int BankAccountId { get; set; }
}