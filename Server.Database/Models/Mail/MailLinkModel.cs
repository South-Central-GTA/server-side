using System.Text.Json;
using AltV.Net;

namespace Server.Database.Models.Mail;

public class MailLinkModel : IWritable
{
    public string MailAccountModelMailAddress { get; set; }
    public MailAccountModel MailAccountModel { get; set; }

    public int MailModelId { get; set; }
    public MailModel MailModel { get; set; }
    public bool IsAuthor { get; set; }

    public void OnWrite(IMValueWriter writer)
    {
        Serialize(this, writer);
    }

    public static void Serialize(MailLinkModel model, IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("id");
        writer.Value(model.MailModelId);

        writer.Name("senderMailAddress");
        writer.Value(model.MailModel.SenderMailAddress);

        writer.Name("title");
        writer.Value(model.MailModel.Title);

        writer.Name("context");
        writer.Value(model.MailModel.Context);

        writer.Name("isAuthor");
        writer.Value(model.IsAuthor);

        writer.Name("sendetAtJson");
        writer.Value(JsonSerializer.Serialize(model.MailModel.CreatedAt));

        writer.EndObject();
    }
}