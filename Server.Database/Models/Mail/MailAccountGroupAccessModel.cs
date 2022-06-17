using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AltV.Net;
using Server.Database.Models.Group;

namespace Server.Database.Models.Mail;

public class MailAccountGroupAccessModel : IWritable
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string MailAccountModelMailAddress { get; set; }

    public MailAccountModel MailAccountModel { get; set; }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int GroupModelId { get; set; }

    public GroupModel? GroupModel { get; set; }

    public bool Owner { get; set; }

    public void OnWrite(IMValueWriter writer)
    {
        Serialize(this, writer);
    }

    public static void Serialize(MailAccountGroupAccessModel model, IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("groupId");
        writer.Value(model.GroupModelId);

        writer.Name("groupName");
        writer.Value(model.GroupModel == null ? string.Empty : model.GroupModel.Name);

        writer.Name("owner");
        writer.Value(model.Owner);

        writer.EndObject();
    }
}