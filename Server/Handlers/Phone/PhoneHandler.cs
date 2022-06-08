using System;
using System.Collections.Generic;
using System.Linq;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Database.Models.Inventory.Phone;
using Server.Helper;
using Server.Modules.Phone;

namespace Server.Handlers.Phone;

public class PhoneHandler : ISingletonScript
{
    private readonly PhoneChatService _phoneChatService;
    private readonly PhoneContactService _phoneContactService;
    private readonly PhoneMessageService _phoneMessageService;
    private readonly PhoneModule _phoneModule;
    private readonly PhoneNotificationService _phoneNotificationService;
    private readonly Serializer _serializer;

    public PhoneHandler(
        PhoneModule phoneModule,
        PhoneChatService phoneChatService,
        PhoneContactService phoneContactService,
        PhoneNotificationService phoneNotificationService,
        PhoneMessageService phoneMessageService,
        Serializer serializer)
    {
        _phoneModule = phoneModule;
        _phoneChatService = phoneChatService;
        _phoneContactService = phoneContactService;
        _phoneNotificationService = phoneNotificationService;
        _phoneMessageService = phoneMessageService;
        _serializer = serializer;

        AltAsync.OnClient<ServerPlayer, int>("phone:requestopen", OnRequestOpen);
        AltAsync.OnClient<ServerPlayer, int>("phone:setasactive", OnSetAsActive);

        AltAsync.OnClient<ServerPlayer, int, int, string>("phone:pushmessage", OnPushMessage);

        AltAsync.OnClient<ServerPlayer, int, int>("phone:updatelastusage", OnUpdateLastUsageChat);
        AltAsync.OnClient<ServerPlayer, int, string>("phone:addchat", OnAddChat);
        AltAsync.OnClient<ServerPlayer, int>("phone:deletechat", OnDeleteChat);

        AltAsync.OnClient<ServerPlayer, int, string>("phone:addcontact", OnAddContact);
        AltAsync.OnClient<ServerPlayer, int, string>("phone:editcontact", OnEditContact);
        AltAsync.OnClient<ServerPlayer, int, int>("phone:removecontact", OnRemoveContact);

        AltAsync.OnClient<ServerPlayer, int, int>("phone:selectbackground", OnSelectBackground);

        AltAsync.OnClient<ServerPlayer, int, int>("phone:deletenotification", OnDeleteNotification);
        AltAsync.OnClient<ServerPlayer, int>("phone:opennotifications", OnOpenNotifications);
    }

    private async void OnRequestOpen(ServerPlayer player, int itemId)
    {
        var phoneItem = player.CharacterModel.InventoryModel.Items.Find(i => i.Id == itemId);
        if (phoneItem == null)
        {
            player.SendNotification("Dein Charakter hat kein Handy im Inventar oder es ist noch nicht aktiv gesetzt.",
                                    NotificationType.ERROR);
            return;
        }

        if (!phoneItem.IsBought)
        {
            player.SendNotification("Dein Charakter kann das Handy erst nach dem Kaufen (oder Diebstahl) nutzen.",
                                    NotificationType.ERROR);
            return;
        }

        await _phoneModule.OpenPhone(player, phoneItem.Id);
    }

    private async void OnSetAsActive(ServerPlayer player, int itemId)
    {
        if (!player.Exists)
        {
            return;
        }

        var phoneItem = await _phoneModule.GetById(itemId);
        if (phoneItem == null || player.CharacterModel.InventoryModel.Items.All(i => i.Id != phoneItem.Id))
        {
            player.SendNotification("Dein Charakter hat kein Handy im Inventar.", NotificationType.ERROR);
            return;
        }

        if (!phoneItem.IsBought)
        {
            player.SendNotification("Dies würde dein Charakter nicht tun.", NotificationType.ERROR);
            return;
        }

        player.SendNotification(
            "Du kannst dieses Handy per Schnellzugriff nun mit Doppel-Pfeiltaste nach oben rausholen.",
            NotificationType.INFO);

        await _phoneModule.SetActive(player, phoneItem.Id);
        await _phoneModule.OpenPhone(player, phoneItem.Id);
    }

    private async void OnPushMessage(ServerPlayer player, int phoneId, int chatId, string context)
    {
        if (!player.Exists)
        {
            return;
        }

        // First send the message to the sender phone.
        var senderPhone = await _phoneModule.GetById(phoneId);
        var chat = senderPhone?.Chats.FirstOrDefault(c => c.Id == chatId);

        if (chat == null)
        {
            return;
        }

        await _phoneMessageService.Add(new PhoneMessageModel
        {
            ChatModelId = chatId,
            OwnerId = player.CharacterModel.Id,
            Context = context,
            Local = true,
            SenderPhoneNumber = senderPhone.PhoneNumber,
            TargetPhoneNumber = chat.PhoneNumber
        });

        chat.LastUsage = DateTime.Now;
        await _phoneChatService.Update(chat);

        await _phoneModule.TryToSendMessage(senderPhone, chat.PhoneNumber, player.CharacterModel.Id, context);

        await _phoneModule.UpdateUi(player, senderPhone.Id);
    }

    private async void OnUpdateLastUsageChat(ServerPlayer player, int phoneId, int chatId)
    {
        if (!player.Exists)
        {
            return;
        }

        var phone = await _phoneModule.GetById(phoneId);
        var chat = phone?.Chats.FirstOrDefault(c => c.Id == chatId);
        if (chat == null)
        {
            return;
        }

        chat.LastUsage = DateTime.Now;

        await _phoneChatService.Update(chat);
        await _phoneModule.UpdateUi(player, phone.Id);
    }

    private async void OnAddChat(ServerPlayer player, int phoneId, string chatJson)
    {
        if (!player.Exists)
        {
            return;
        }

        var phoneChat = _serializer.Deserialize<PhoneChatModel>(chatJson);
        var phone = await _phoneModule.GetById(phoneId);
        if (phoneChat.PhoneNumber == phone?.PhoneNumber)
        {
            player.SendNotification("Dein Charakter kann kein Chat mit sich selber eröffnen.", NotificationType.ERROR);
            return;
        }

        var chat = await _phoneChatService.Add(new PhoneChatModel
        {
            ItemPhoneModelId = phoneId,
            PhoneNumber = phoneChat.PhoneNumber,
            Name = phoneChat.Name,
            Messages = new List<PhoneMessageModel>()
        });

        lock (player)
        {
            player.EmitLocked("phone:opennewchat", phoneChat.Id, chat);
        }
    }

    private async void OnDeleteChat(ServerPlayer player, int chatId)
    {
        var phoneChat = await _phoneChatService.GetByKey(chatId);

        await _phoneChatService.Remove(phoneChat);
    }

    private async void OnAddContact(ServerPlayer player, int phoneId, string contactJson)
    {
        if (!player.Exists)
        {
            return;
        }

        var phoneContact = _serializer.Deserialize<PhoneContactModel>(contactJson);
        var phone = await _phoneModule.GetById(phoneId);
        if (phone == null)
        {
            return;
        }

        if (phone.PhoneNumber == phoneContact.PhoneNumber)
        {
            player.SendNotification("Dein Charakter kann sich nicht selbst im Telefonbuch abspeichern.",
                                    NotificationType.ERROR);
            return;
        }

        var contact = phone.Contacts.FirstOrDefault(c => c.PhoneNumber == phoneContact.PhoneNumber
                                                         || c.Name == phoneContact.Name);
        if (contact != null)
        {
            player.SendNotification(
                $"Dein Charakter hat die Nummer '{contact.PhoneNumber}' schon unter dem Kontakt '{contact.Name}' abgespeichert.",
                NotificationType.ERROR);
            return;
        }

        await _phoneContactService.Add(new PhoneContactModel
        {
            ItemPhoneModelId = phone.Id,
            PhoneNumber = phoneContact.PhoneNumber,
            Name = phoneContact.Name
        });

        var chatDbo = phone.Chats.FirstOrDefault(c => c.PhoneNumber == phoneContact.PhoneNumber);
        if (chatDbo != null)
        {
            chatDbo.Name = phoneContact.Name;
            await _phoneChatService.Update(chatDbo);
        }

        await _phoneModule.UpdateUi(player, phone.Id);
    }

    private async void OnEditContact(ServerPlayer player, int phoneId, string contactJson)
    {
        if (!player.Exists)
        {
            return;
        }

        var phoneContact = _serializer.Deserialize<PhoneContactModel>(contactJson);
        var contactDbo = await _phoneContactService.GetByKey(phoneContact.Id);

        contactDbo.Name = phoneContact.Name;
        contactDbo.PhoneNumber = phoneContact.PhoneNumber;

        await _phoneContactService.Update(contactDbo);

        var phone = await _phoneModule.GetById(phoneId);
        if (phone == null)
        {
            return;
        }

        var chat = phone.Chats.FirstOrDefault(c => c.PhoneNumber == phoneContact.PhoneNumber);
        if (chat != null)
        {
            chat.Name = phoneContact.Name;
            await _phoneChatService.Update(chat);
        }

        await _phoneModule.UpdateUi(player, phone.Id);
    }

    private async void OnRemoveContact(ServerPlayer player, int phoneId, int contactId)
    {
        if (!player.Exists)
        {
            return;
        }

        var deletedContact = await _phoneContactService.GetByKey(contactId);

        var phone = await _phoneModule.GetById(phoneId);
        if (phone == null)
        {
            return;
        }

        var chat = phone.Chats.FirstOrDefault(c => c.PhoneNumber == deletedContact.PhoneNumber);
        if (chat != null)
        {
            chat.Name = deletedContact.PhoneNumber;
            await _phoneChatService.Update(chat);
        }

        await _phoneContactService.Remove(deletedContact);

        await _phoneModule.UpdateUi(player, phone.Id);
    }

    private async void OnSelectBackground(ServerPlayer player, int phoneId, int backgroundId)
    {
        if (!player.Exists)
        {
            return;
        }

        var phone = await _phoneModule.GetById(phoneId);
        if (phone == null)
        {
            return;
        }

        phone.BackgroundImageId = backgroundId;
        await _phoneModule.UpdatePhone(phone);

        await _phoneModule.UpdateUi(player, phone.Id);
    }

    private async void OnDeleteNotification(ServerPlayer player, int phoneId, int notificationId)
    {
        if (!player.Exists)
        {
            return;
        }

        var phoneNotification = await _phoneNotificationService.GetByKey(notificationId);
        await _phoneNotificationService.Remove(phoneNotification);

        await _phoneModule.UpdateUi(player, phoneNotification.ItemPhoneModelId);
    }

    private async void OnOpenNotifications(ServerPlayer player, int phoneId)
    {
        if (!player.Exists)
        {
            return;
        }

        var phone = await _phoneModule.GetById(phoneId);
        if (phone == null)
        {
            return;
        }

        phone.LastTimeOpenedNotifications = DateTime.Now;

        await _phoneModule.UpdatePhone(phone);

        await _phoneModule.UpdateUi(player, phone.Id);
    }
}