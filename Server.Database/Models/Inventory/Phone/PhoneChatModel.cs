using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using AltV.Net;
using Server.Database.Models._Base;

namespace Server.Database.Models.Inventory.Phone;

public class PhoneChatModel : ModelBase, IWritable
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
        Serialize(this, writer);
    }

    public static void Serialize(PhoneChatModel model, IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("id");
        writer.Value(model.Id);

        writer.Name("phoneNumber");
        writer.Value(model.PhoneNumber);

        writer.Name("name");
        writer.Value(model.Name);

        writer.Name("lastUsageJson");
        writer.Value(JsonSerializer.Serialize(model.LastUsage));

        writer.Name("messages");

        writer.BeginArray();

        foreach (var message in model.Messages)
        {
            PhoneMessageModel.Serialize(message, writer);
        }

        writer.EndArray();

        writer.EndObject();
    }
}