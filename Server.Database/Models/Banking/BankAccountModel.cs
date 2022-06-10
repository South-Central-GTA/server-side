using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AltV.Net;
using Server.Database.Enums;
using Server.Database.Models._Base;

namespace Server.Database.Models.Banking;

public class BankAccountModel : ModelBase, IWritable
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    public BankAccountState Status { set; get; }
    public OwnableAccountType Type { set; get; }

    public long Amount { get; set; }
    public string BankDetails { get; set; }

    public List<BankAccountCharacterAccessModel> CharacterAccesses { get; init; } = new();
    public List<BankAccountGroupRankAccessModel> GroupRankAccess { get; init; } = new();
    public List<BankHistoryEntryModel> History { get; init; } = new();

    public void OnWrite(IMValueWriter writer)
    {
        Serialize(this, writer);
    }

    public static void Serialize(BankAccountModel model, IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("id");
        writer.Value(model.Id);

        writer.Name("status");
        writer.Value((int)model.Status);

        writer.Name("type");
        writer.Value((int)model.Type);

        writer.Name("amount");
        writer.Value(model.Amount.ToString());

        writer.Name("bankDetails");
        writer.Value(model.BankDetails);

        writer.Name("characterAccesses");

        writer.BeginArray();

        foreach (var characterAccess in model.CharacterAccesses)
        {
            BankAccountCharacterAccessModel.Serialize(characterAccess, writer);
        }

        writer.EndArray();

        writer.Name("groupAccesses");

        writer.BeginArray();

        foreach (var groupAccess in model.GroupRankAccess)
        {
            BankAccountGroupRankAccessModel.Serialize(groupAccess, writer);
        }

        writer.EndArray();

        writer.Name("history");

        writer.BeginArray();

        foreach (var historyEntry in model.History)
        {
            BankHistoryEntryModel.Serialize(historyEntry, writer);
        }

        writer.EndArray();

        writer.EndObject();
    }
}