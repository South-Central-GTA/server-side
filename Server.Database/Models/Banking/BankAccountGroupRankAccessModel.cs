using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Server.Database.Models.Group;

namespace Server.Database.Models.Banking;

public class BankAccountGroupRankAccessModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int BankAccountModelId { get; set; }

    public BankAccountModel BankAccountModel { get; set; }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int GroupModelId { get; set; }

    public GroupModel? GroupModel { get; set; }

    public bool Owner { get; set; }
}