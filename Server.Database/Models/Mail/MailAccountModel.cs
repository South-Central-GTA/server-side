using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using AltV.Net;
using Server.Database.Enums;
using Server.Database.Models._Base;

namespace Server.Database.Models.Mail;

public class MailAccountModel
    : ModelBase, IWritable
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

        if (CharacterAccesses != null)
        {
            for (var i = 0; i < CharacterAccesses.Count; i++)
            {
                writer.BeginObject();

                var characterAccesses = CharacterAccesses[i];

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

        if (GroupAccess != null)
        {
            for (var i = 0; i < GroupAccess.Count; i++)
            {
                writer.BeginObject();

                var groupAccesses = GroupAccess[i];

                writer.Name("groupId");
                writer.Value(groupAccesses.GroupModelId);

                writer.Name("groupName");
                writer.Value(groupAccesses.GroupModel != null ? groupAccesses.GroupModel.Name : "");

                writer.Name("owner");
                writer.Value(groupAccesses.Owner);

                writer.EndObject();
            }
        }

        writer.EndArray();

        writer.Name("mails");

        writer.BeginArray();

        if (MailLinks != null)
        {
            for (var i = 0; i < MailLinks.Count; i++)
            {
                writer.BeginObject();

                var link = MailLinks[i];

                writer.Name("id");
                writer.Value(link.MailModelId);

                writer.Name("senderMailAddress");
                writer.Value(link.MailModel.SenderMailAddress);

                writer.Name("title");
                writer.Value(link.MailModel.Title);

                writer.Name("context");
                writer.Value(link.MailModel.Context);

                writer.Name("isAuthor");
                writer.Value(link.IsAuthor);

                writer.Name("sendetAtJson");
                writer.Value(JsonSerializer.Serialize(link.MailModel.CreatedAt));

                writer.EndObject();
            }
        }

        writer.EndArray();

        writer.EndObject();
    }
}