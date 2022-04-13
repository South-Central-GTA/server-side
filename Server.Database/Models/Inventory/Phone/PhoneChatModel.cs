using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using AltV.Net;
using Server.Database.Models._Base;

namespace Server.Database.Models.Inventory.Phone;

public class PhoneChatModel
    : ModelBase, IWritable
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    public int ItemPhoneModelId { get; set; }
    public ItemPhoneModel ItemPhoneModel { get; set; }

    public List<PhoneMessageModel> Messages { get; init; } = new();

    public string PhoneNumber { get; set; }
    public string Name { get; set; }

    public void OnWrite(IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("id");
        writer.Value(Id);

        writer.Name("phoneNumber");
        writer.Value(PhoneNumber);

        writer.Name("name");
        writer.Value(Name);

        writer.Name("lastUsage");
        writer.Value(JsonSerializer.Serialize(LastUsage));

        writer.Name("messages");

        writer.BeginArray();

        if (Messages != null)
        {
            for (var m = 0; m < Messages.Count; m++)
            {
                writer.BeginObject();

                var message = Messages[m];

                writer.Name("id");
                writer.Value(message.Id);

                writer.Name("chatId");
                writer.Value(message.ChatModelId);

                writer.Name("sendetAt");
                writer.Value(JsonSerializer.Serialize(message.CreatedAt));

                writer.Name("ownerId");
                writer.Value(message.OwnerId);

                writer.Name("context");
                writer.Value(message.Context);

                writer.Name("local");
                writer.Value(message.Local);

                writer.Name("senderPhoneNumber");
                writer.Value(message.SenderPhoneNumber);

                writer.Name("targetPhoneNumber");
                writer.Value(message.TargetPhoneNumber);

                writer.EndObject();
            }
        }

        writer.EndArray();

        writer.EndObject();
    }
}