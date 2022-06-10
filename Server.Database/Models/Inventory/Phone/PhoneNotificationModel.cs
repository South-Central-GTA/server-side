using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using AltV.Net;
using Server.Database.Enums;
using Server.Database.Models._Base;

namespace Server.Database.Models.Inventory.Phone;

public class PhoneNotificationModel : ModelBase, IWritable
{
    public PhoneNotificationModel(int itemPhoneModelId, PhoneNotificationType type, string context)
    {
        ItemPhoneModelId = itemPhoneModelId;
        Type = type;
        Context = context;
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    public int ItemPhoneModelId { get; set; }
    public ItemPhoneModel ItemPhoneModel { get; set; }

    public string Context { get; set; }
    public PhoneNotificationType Type { get; set; }

    public void OnWrite(IMValueWriter writer)
    {
        Serialize(this, writer);
    }

    public static void Serialize(PhoneNotificationModel model, IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("id");
        writer.Value(model.Id);

        writer.Name("context");
        writer.Value(model.Context);

        writer.Name("type");
        writer.Value((int)model.Type);

        writer.Name("createdAtJson");
        writer.Value(JsonSerializer.Serialize(model.CreatedAt));

        writer.EndObject();
    }
}