using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using AltV.Net;
using Server.Database.Models.Inventory.Phone;

namespace Server.Database.Models.Inventory;

public class ItemPhoneModel : ItemModel
{
    public string PhoneNumber { get; set; }
    public bool Active { get; set; }

    public int BackgroundImageId { get; set; }
    public int? CurrentOwnerId { get; set; }

    public int InitialOwnerId { get; set; }

    [JsonIgnore] public DateTime LastTimeOpenedNotifications { get; set; }

    public List<PhoneContactModel> Contacts { get; set; } = new();
    public List<PhoneChatModel> Chats { get; set; } = new();
    public List<PhoneNotificationModel> Notifications { get; set; } = new();

    public override void OnWrite(IMValueWriter writer)
    {
        Serialize(this, writer);
    }

    public static void Serialize(ItemPhoneModel model, IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("id");
        writer.Value(model.Id);

        writer.Name("phoneNumber");
        writer.Value(model.PhoneNumber);

        writer.Name("active");
        writer.Value(model.Active);

        writer.Name("backgroundImageId");
        writer.Value(model.BackgroundImageId);

        writer.Name("ownerId");
        writer.Value(model.CurrentOwnerId ?? -1);

        writer.Name("lastTimeOpenedNotificationsJson");
        writer.Value(JsonSerializer.Serialize(model.LastTimeOpenedNotifications));

        writer.Name("contacts");

        writer.BeginArray();

        foreach (var contact in model.Contacts)
        {
            PhoneContactModel.Serialize(contact, writer);
        }

        writer.EndArray();

        writer.Name("chats");

        writer.BeginArray();

        foreach (var chat in model.Chats)
        {
            PhoneChatModel.Serialize(chat, writer);
        }

        writer.EndArray();

        writer.Name("notifications");

        writer.BeginArray();

        foreach (var notification in model.Notifications)
        {
            PhoneNotificationModel.Serialize(notification, writer);
        }

        writer.EndArray();

        writer.Name("catalogItemName");
        writer.Value(model.CatalogItemModelId.ToString());

        writer.Name("catalogItem");

        CatalogItemModel.Serialize(model.CatalogItemModel, writer);
        
        writer.EndObject();
    }
}