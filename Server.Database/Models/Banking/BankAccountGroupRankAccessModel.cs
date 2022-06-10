using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AltV.Net;
using Server.Database.Models.Group;

namespace Server.Database.Models.Banking;

public class BankAccountGroupRankAccessModel : IWritable
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

    public void OnWrite(IMValueWriter writer)
    {
        Serialize(this, writer);
    }

    public static void Serialize(BankAccountGroupRankAccessModel model, IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("groupId");
        writer.Value(model.GroupModelId);

        writer.Name("name");
        writer.Value(model.GroupModel != null ? model.GroupModel.Name : string.Empty);

        writer.Name("owner");
        writer.Value(model.Owner);

        writer.EndObject();
    }
}