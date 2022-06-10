using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using AltV.Net;
using Server.Database.Models._Base;

namespace Server.Database.Models.Mail;

public class MailModel : ModelBase, IWritable
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    public List<MailLinkModel> MailLinks { get; init; } = new();

    public string SenderMailAddress { get; set; }
    public List<string> MailReadedFromAddress { get; init; } = new();

    [MaxLength(50)] public string Title { get; set; }

    public string Context { get; set; }

    public void OnWrite(IMValueWriter writer)
    {
        Serialize(this, writer);
    }

    public static void Serialize(MailModel model, IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("id");
        writer.Value(model.Id);

        writer.Name("senderMailAddress");
        writer.Value(model.SenderMailAddress);

        writer.Name("title");
        writer.Value(model.Title);

        writer.Name("context");
        writer.Value(model.Context);

        writer.Name("sendetAtJson");
        writer.Value(JsonSerializer.Serialize(model.CreatedAt));

        writer.EndObject();
    }
}