using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using AltV.Net;
using Server.Database.Models.Inventory.Phone;

namespace Server.Database.Models.Inventory;

public class ItemPhoneModel
    : ItemModel, IWritable
{
    public string PhoneNumber { get; set; }
    public bool Active { get; set; }

    public int BackgroundImageId { get; set; }
    public int? CurrentOwnerId { get; set; }
    
    public int InitialOwnerId { get; set; }

    [JsonIgnore] public DateTime LastTimeOpenedNotifications { get; set; }

    public List<PhoneContactModel> Contacts { get; set; } = new ();
    public List<PhoneChatModel> Chats { get; set; } = new ();
    public List<PhoneNotificationModel> Notifications { get; set; } = new ();

    public override void OnWrite(IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("id");
        writer.Value(Id);

        writer.Name("phoneNumber");
        writer.Value(PhoneNumber);

        writer.Name("active");
        writer.Value(Active);

        writer.Name("backgroundImageId");
        writer.Value(BackgroundImageId);

        writer.Name("ownerId");
        writer.Value(CurrentOwnerId.HasValue ? CurrentOwnerId.Value : -1);

        writer.Name("lastTimeOpendNotifications");
        writer.Value(JsonSerializer.Serialize(LastTimeOpenedNotifications));

        writer.Name("contacts");

        writer.BeginArray();

        if (Contacts != null)
        {
            for (var i = 0; i < Contacts.Count; i++)
            {
                writer.BeginObject();

                var contact = Contacts[i];

                writer.Name("id");
                writer.Value(contact.Id);

                writer.Name("phoneNumber");
                writer.Value(contact.PhoneNumber);

                writer.Name("name");
                writer.Value(contact.Name);

                writer.EndObject();
            }
        }

        writer.EndArray();

        writer.Name("chats");

        writer.BeginArray();

        if (Chats != null)
        {
            for (var i = 0; i < Chats.Count; i++)
            {
                writer.BeginObject();

                var chat = Chats[i];

                writer.Name("id");
                writer.Value(chat.Id);

                writer.Name("phoneNumber");
                writer.Value(chat.PhoneNumber);

                writer.Name("name");
                writer.Value(chat.Name);

                writer.Name("lastUsage");
                writer.Value(JsonSerializer.Serialize(chat.LastUsage));

                writer.Name("messages");

                writer.BeginArray();

                if (chat.Messages != null)
                {
                    for (var m = 0; m < chat.Messages.Count; m++)
                    {
                        writer.BeginObject();

                        var message = chat.Messages[m];

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

        writer.EndArray();

        writer.Name("notifications");

        writer.BeginArray();

        if (Notifications != null)
        {
            for (var i = 0; i < Notifications.Count; i++)
            {
                writer.BeginObject();

                var notification = Notifications[i];

                writer.Name("id");
                writer.Value(notification.Id);

                writer.Name("context");
                writer.Value(notification.Context);

                writer.Name("type");
                writer.Value((int)notification.Type);

                writer.Name("createdAt");
                writer.Value(JsonSerializer.Serialize(notification.CreatedAt));

                writer.EndObject();
            }
        }

        writer.EndArray();

        writer.Name("catalogItemName");
        writer.Value(CatalogItemModelId.ToString());

        writer.Name("catalogItem");

        writer.BeginObject();

        writer.Name("id");
        writer.Value((int)CatalogItemModel.Id);

        writer.Name("name");
        writer.Value(CatalogItemModel.Name);

        writer.Name("image");
        writer.Value(CatalogItemModel.Image);

        writer.Name("description");
        writer.Value(CatalogItemModel.Description);

        writer.Name("rarity");
        writer.Value((int)CatalogItemModel.Rarity);

        writer.Name("weight");
        writer.Value(CatalogItemModel.Weight);

        writer.Name("equippable");
        writer.Value(CatalogItemModel.Equippable);

        writer.Name("stackable");
        writer.Value(CatalogItemModel.Stackable);

        writer.Name("buyable");
        writer.Value(CatalogItemModel.Buyable);

        writer.Name("sellable");
        writer.Value(CatalogItemModel.Sellable);

        writer.Name("price");
        writer.Value(CatalogItemModel.Price);

        writer.Name("sellPrice");
        writer.Value(CatalogItemModel.SellPrice);

        writer.EndObject();

        writer.Name("slot");
        writer.Value(Slot ?? -1);

        writer.Name("droppedByCharacter");
        writer.Value(DroppedByCharacter ?? "Unbekannt");

        writer.Name("customData");
        writer.Value(CustomData);

        writer.Name("note");
        writer.Value(Note);

        writer.Name("amount");
        writer.Value(Amount);

        writer.Name("condition");
        writer.Value(Condition ?? -1);

        writer.Name("isBought");
        writer.Value(IsBought);

        writer.Name("itemState");
        writer.Value((int)ItemState);

        writer.Name("positionX");
        writer.Value(PositionX);

        writer.Name("positionY");
        writer.Value(PositionY);

        writer.Name("positionZ");
        writer.Value(PositionZ);

        writer.Name("lastUsage");
        writer.Value(JsonSerializer.Serialize(LastUsage));

        writer.EndObject();
    }
}