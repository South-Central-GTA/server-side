using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Server.Database.Enums;
using Server.Database.Models._Base;

namespace Server.Database.Models.Banking;

public class BankHistoryEntryModel
    : ModelBase
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    public int BankAccountModelId { get; set; }
    public BankAccountModel BankAccountModel { get; set; }

    public BankHistoryType HistoryType { get; set; }

    public bool Income { get; set; }
    public int Amount { get; set; }
    public string PurposeOfUse { get; set; }
}