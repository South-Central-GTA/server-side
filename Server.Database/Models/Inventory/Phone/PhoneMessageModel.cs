using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;
using AltV.Net;
using Server.Database.Models._Base;

namespace Server.Database.Models.Inventory.Phone;

public class PhoneMessageModel
    : ModelBase, IWritable
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
        writer.BeginObject();

        writer.Name("id");
        writer.Value(Id);

        writer.Name("chatId");
        writer.Value(ChatModelId);

        writer.Name("sendetAt");
        writer.Value(JsonSerializer.Serialize(CreatedAt));

        writer.Name("ownerId");
        writer.Value(OwnerId);

        writer.Name("context");
        writer.Value(Context);

        writer.Name("senderPhoneNumber");
        writer.Value(SenderPhoneNumber);

        writer.Name("targetPhoneNumber");
        writer.Value(TargetPhoneNumber);

        writer.EndObject();
    }
}