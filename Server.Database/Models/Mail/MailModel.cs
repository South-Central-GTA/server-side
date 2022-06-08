using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using AltV.Net;
using Server.Database.Models._Base;

namespace Server.Database.Models.Mail;

public class MailModel
    : ModelBase, IWritable
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
        writer.BeginObject();

        writer.Name("id");
        writer.Value(Id);

        writer.Name("senderMailAddress");
        writer.Value(SenderMailAddress);

        writer.Name("title");
        writer.Value(Title);

        writer.Name("context");
        writer.Value(Context);

        writer.Name("sendetAtJson");
        writer.Value(JsonSerializer.Serialize(CreatedAt));

        writer.EndObject();
    }
}