using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Server.Database.Models._Base;

namespace Server.Database.Models.Housing;

public class UserShopDataModel
    : ModelBase
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int CharacterModelId { get; set; }

    public bool GotWarned { get; set; }
    public int BillToPay { get; set; }
}