using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using AltV.Net;
using Server.Database.Models._Base;

namespace Server.Database.Models.Inventory.Phone;

public class PhoneMessageModel : ModelBase, IWritable
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    public int ChatModelId { get; set; }
    public PhoneChatModel ChatModel { get; set; }

    public int OwnerId { get; set; }
    public string Context { get; set; }
    public bool Local { get; set; }

    public string SenderPhoneNumber { get; set; }
    public string TargetPhoneNumber { get; set; }

    public void OnWrite(IMValueWriter writer)
    {
        Serialize(this, writer);
    }

    public static void Serialize(PhoneMessageModel model, IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("id");
        writer.Value(model.Id);

        writer.Name("chatId");
        writer.Value(model.ChatModelId);

        writer.Name("sendetAt");
        writer.Value(JsonSerializer.Serialize(model.CreatedAt));

        writer.Name("ownerId");
        writer.Value(model.OwnerId);

        writer.Name("context");
        writer.Value(model.Context);

        writer.Name("local");
        writer.Value(model.Local);

        writer.Name("senderPhoneNumber");
        writer.Value(model.SenderPhoneNumber);

        writer.Name("targetPhoneNumber");
        writer.Value(model.TargetPhoneNumber);

        writer.EndObject();
    }
}