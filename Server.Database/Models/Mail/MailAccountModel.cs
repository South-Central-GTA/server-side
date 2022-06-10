using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AltV.Net;
using Server.Database.Enums;
using Server.Database.Models._Base;

namespace Server.Database.Models.Mail;

public class MailAccountModel : ModelBase, IWritable
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string MailAddress { get; set; }

    public OwnableAccountType Type { set; get; }

    public List<MailLinkModel> MailLinks { get; init; } = new();
    public List<MailAccountCharacterAccessModel> CharacterAccesses { get; init; } = new();
    public List<MailAccountGroupAccessModel> GroupAccess { get; init; } = new();

    public void OnWrite(IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("type");
        writer.Value((int)Type);

        writer.Name("mailAddress");
        writer.Value(MailAddress);

        writer.Name("characterAccesses");

        writer.BeginArray();

        foreach (var characterAccesses in CharacterAccesses)
        {
            MailAccountCharacterAccessModel.Serialize(characterAccesses, writer);
        }

        writer.EndArray();

        writer.Name("groupAccesses");

        writer.BeginArray();

        foreach (var groupAccess in GroupAccess)
        {
            MailAccountGroupAccessModel.Serialize(groupAccess, writer);
        }

        writer.EndArray();

        writer.Name("mails");

        writer.BeginArray();

        foreach (var link in MailLinks)
        {
            MailLinkModel.Serialize(link, writer);
        }

        writer.EndArray();

        writer.EndObject();
    }
}