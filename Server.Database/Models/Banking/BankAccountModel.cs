using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using AltV.Net;
using Server.Database.Enums;
using Server.Database.Models._Base;

namespace Server.Database.Models.Banking;

public class BankAccountModel
    : ModelBase, IWritable
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
        writer.BeginObject();

        writer.Name("id");
        writer.Value(Id);

        writer.Name("status");
        writer.Value((int)Status);

        writer.Name("type");
        writer.Value((int)Type);

        writer.Name("amount");
        writer.Value(Amount.ToString());

        writer.Name("bankDetails");
        writer.Value(BankDetails);

        writer.Name("characterAccesses");

        writer.BeginArray();

        if (CharacterAccesses != null)
        {
            for (var i = 0; i < CharacterAccesses.Count; i++)
            {
                writer.BeginObject();

                var characterAccesses = CharacterAccesses[i];

                writer.Name("bankAccountId");
                writer.Value(characterAccesses.BankAccountModelId);

                writer.Name("permission");
                writer.Value((int)characterAccesses.Permission);

                writer.Name("name");
                writer.Value(characterAccesses.CharacterModel.Name);

                writer.Name("characterId");
                writer.Value(characterAccesses.CharacterModel.Id);

                writer.Name("owner");
                writer.Value(characterAccesses.Owner);

                writer.EndObject();
            }
        }

        writer.EndArray();

        writer.Name("groupAccesses");

        writer.BeginArray();

        if (GroupRankAccess != null)
        {
            for (var i = 0; i < GroupRankAccess.Count; i++)
            {
                writer.BeginObject();

                var groupAccesses = GroupRankAccess[i];

                writer.Name("groupId");
                writer.Value(groupAccesses.GroupModelId);

                writer.Name("name");
                writer.Value(groupAccesses.GroupModel != null ? groupAccesses.GroupModel.Name : "");

                writer.Name("owner");
                writer.Value(groupAccesses.Owner);

                writer.EndObject();
            }
        }

        writer.EndArray();

        writer.Name("history");

        writer.BeginArray();

        if (History != null)
        {
            foreach (var historyEntry in History)
            {
                writer.BeginObject();

                writer.Name("id");
                writer.Value(historyEntry.Id);

                writer.Name("type");
                writer.Value((int)historyEntry.HistoryType);

                writer.Name("income");
                writer.Value(historyEntry.Income);

                writer.Name("amount");
                writer.Value(historyEntry.Amount);

                writer.Name("purposeOfUse");
                writer.Value(historyEntry.PurposeOfUse);

                writer.Name("sendetAtJson");
                writer.Value(JsonSerializer.Serialize(historyEntry.CreatedAt));

                writer.EndObject();
            }
        }

        writer.EndArray();

        writer.EndObject();
    }
}