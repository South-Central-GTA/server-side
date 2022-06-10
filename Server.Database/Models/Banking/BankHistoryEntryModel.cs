using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using AltV.Net;
using Server.Database.Enums;
using Server.Database.Models._Base;

namespace Server.Database.Models.Banking;

public class BankHistoryEntryModel : ModelBase, IWritable
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

    public void OnWrite(IMValueWriter writer)
    {
        Serialize(this, writer);
    }

    public static void Serialize(BankHistoryEntryModel model, IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("id");
        writer.Value(model.Id);

        writer.Name("type");
        writer.Value((int)model.HistoryType);

        writer.Name("income");
        writer.Value(model.Income);

        writer.Name("amount");
        writer.Value(model.Amount);

        writer.Name("purposeOfUse");
        writer.Value(model.PurposeOfUse);

        writer.Name("sendetAtJson");
        writer.Value(JsonSerializer.Serialize(model.CreatedAt));

        writer.EndObject();
    }
}