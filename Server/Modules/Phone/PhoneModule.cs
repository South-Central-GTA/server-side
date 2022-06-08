using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Async;
using Microsoft.Extensions.Logging;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Inventory;
using Server.Database.Models.Inventory.Phone;
using Server.Modules.Mail;

namespace Server.Modules.Phone;

public class PhoneModule : ISingletonScript
{
    private readonly ItemPhoneService _itemPhoneService;
    private readonly ILogger<PhoneModule> _logger;

    private readonly MailModule _mailModule;
    private readonly PhoneChatService _phoneChatService;
    private readonly PhoneMessageService _phoneMessageService;
    private readonly PhoneNotificationService _phoneNotificationService;

    public PhoneModule(
        ILogger<PhoneModule> logger,
        PhoneMessageService phoneMessageService,
        PhoneChatService phoneChatService,
        PhoneNotificationService phoneNotificationService,
        ItemPhoneService itemPhoneService,
        MailModule mailModule)
    {
        _logger = logger;
        _phoneMessageService = phoneMessageService;
        _phoneChatService = phoneChatService;
        _phoneNotificationService = phoneNotificationService;
        _itemPhoneService = itemPhoneService;

        _mailModule = mailModule;
    }

    public async Task OpenPhone(ServerPlayer player, int phoneId)
    {
        if (!player.Exists)
        {
            return;
        }

        if (player.CharacterModel.DeathState == DeathState.DEAD || player.Cuffed)
        {
            player.SendNotification("Du kannst jetzt nicht dein Handy nutzen.", NotificationType.ERROR);
            return;
        }

        var phone = await GetById(phoneId);
        if (phone == null)
        {
            return;
        }

        player.EmitLocked("phone:open", phone);

        await _mailModule.UpdateUi(player);

        // Handle situation if there is currently a call to this phone.
        if (player.PhoneCallData is { IsInitiator: false })
        {
            var callerContact = phone.Contacts.Find(c => c.PhoneNumber == player.PhoneCallData.PartnerPhoneNumber);

            var displayedName = callerContact != null ? callerContact.Name : player.PhoneCallData.PartnerPhoneNumber;

            player.EmitLocked("phone:getcallfrom", displayedName, phone.Id);
        }
    }

    /// Lost connection boolean is true for example when the player dies.
    public async Task HandleDropPhoneItem(ServerPlayer player, ItemPhoneModel phoneModel)
    {
        if (!player.Exists)
        {
            return;
        }

        phoneModel.CurrentOwnerId = null;
        phoneModel.Active = false;

        await UpdatePhone(phoneModel);

        player.EmitLocked("phone:remove");
    }

    public async Task UpdateUi(ServerPlayer player, int phoneId)
    {
        if (!player.Exists)
        {
            return;
        }

        var phone = await GetById(phoneId);
        if (phone == null)
        {
            return;
        }

        player.EmitLocked("phone:update", phone);
    }

    public async Task SetOwner(ServerPlayer player, int itemPhoneId)
    {
        if (!player.Exists)
        {
            return;
        }

        var phone = await GetById(itemPhoneId);
        if (phone == null)
        {
            return;
        }

        phone.CurrentOwnerId = player.CharacterModel.Id;

        await UpdatePhone(phone);
    }

    public async Task TryToSendMessage(ItemPhoneModel senderPhoneModel, string targetNumber, int ownerId,
                                       string context)
    {
        var targetPhone = await GetByNumber(targetNumber);
        if (targetPhone == null)
        {
            return;
        }

        var chat = targetPhone?.Chats.FirstOrDefault(c => c.PhoneNumber == senderPhoneModel.PhoneNumber);

        if (chat != null)
        {
            await _phoneMessageService.Add(new PhoneMessageModel
            {
                ChatModelId = chat.Id,
                OwnerId = ownerId,
                Context = context,
                Local = false,
                SenderPhoneNumber = senderPhoneModel.PhoneNumber,
                TargetPhoneNumber = targetNumber
            });
        }
        else
        {
            // Check if the sender phone number is in the contacts, if take the name, if not just take the number.
            var senderInContacts =
                targetPhone.Contacts.FirstOrDefault(c => c.PhoneNumber == senderPhoneModel.PhoneNumber);
            var displayedName = senderInContacts != null ? senderInContacts.Name : senderPhoneModel.PhoneNumber;

            var messages = new List<PhoneMessageModel>
            {
                new()
                {
                    OwnerId = ownerId,
                    Context = context,
                    Local = false,
                    SenderPhoneNumber = senderPhoneModel.PhoneNumber,
                    TargetPhoneNumber = targetNumber
                }
            };

            await _phoneChatService.Add(new PhoneChatModel
            {
                ItemPhoneModelId = targetPhone.Id,
                PhoneNumber = senderPhoneModel.PhoneNumber,
                Name = displayedName,
                Messages = messages,
                CreatedAt = DateTime.Now
            });
        }

        // Just get the phone from the database to have everything up to date.
        targetPhone = await GetById(targetPhone.Id);

        if (targetPhone?.CurrentOwnerId == null)
        {
            return;
        }

        var contactPlayer = Alt.GetAllPlayers().FindPlayerByCharacterId(targetPhone.CurrentOwnerId.Value);
        if (contactPlayer != null)
        {
            var contactPhoneItem = contactPlayer.CharacterModel.InventoryModel.Items.FirstOrDefault(
                i => i.CustomData == targetPhone.Id.ToString()
                     && i.CatalogItemModelId == ItemCatalogIds.PHONE);

            var handyName = contactPhoneItem?.Note != null
                ? $"Du hast auf dem Handy ({contactPhoneItem.Note})"
                : "Du hast auf deinem Handy";

            contactPlayer.SendNotification($"{handyName} eine SMS bekommen.", NotificationType.INFO);
            contactPlayer.EmitLocked("phone:update", targetPhone);
        }
    }

    public async Task SetActive(ServerPlayer player, int phoneId)
    {
        var phones = await Where(p => p.CurrentOwnerId == player.CharacterModel.Id || p.Id == phoneId);
        phones.ForEach(p => { p.Active = false; });

        var phone = phones.Find(p => p.Id == phoneId);
        if (phone != null)
        {
            phone.Active = true;

            player.EmitLocked("phone:setup", phone);
        }

        await UpdatePhones(phones);
    }

    public async Task<string> GetRandomPhoneNumber(int areaCode = 555)
    {
        var rnd = new Random();
        var number = rnd.Next(10000000, 99999999);

        while (await GetByNumber(number.ToString()) != null)
        {
            number = rnd.Next(10000000, 99999999);
        }

        return areaCode + number.ToString();
    }

    public async Task SendNotification(int phoneId, PhoneNotificationType type, string context)
    {
        await _phoneNotificationService.Add(new PhoneNotificationModel(phoneId, type, context));

        var phoneItem = await GetById(phoneId);
        if (phoneItem?.InventoryModel.CharacterModelId != null)
        {
            var player = Alt.GetAllPlayers().FindPlayerByCharacterId(phoneItem.InventoryModel.CharacterModelId.Value);
            if (player != null)
            {
                var handyName = phoneItem.Note != null ? $"Das Handy ({phoneItem.Note})" : "Das Handy";
                player.SendNotification($"{handyName} deines Charakters hat eine Benachrichtigung erhalten.",
                                        NotificationType.INFO);

                await UpdateUi(player, phoneId);
            }
        }
    }

    public async Task<ItemPhoneModel?> GetByOwner(int characterId)
    {
        var allItems = await _itemPhoneService.GetAll();
        var phone = allItems.FirstOrDefault(i => i.CurrentOwnerId == characterId);

        return phone;
    }

    public async Task<ItemPhoneModel?> GetByNumber(string number)
    {
        var allItems = await _itemPhoneService.GetAll();
        var phone = allItems.FirstOrDefault(i => i.PhoneNumber == number);

        return phone;
    }

    public async Task<ItemPhoneModel?> GetById(int itemPhoneId)
    {
        return await _itemPhoneService.GetByKey(itemPhoneId);
    }

    private async Task<List<ItemPhoneModel>> Where(Expression<Func<ItemPhoneModel, bool>> expression)
    {
        return await _itemPhoneService.Where(expression);
    }

    public async Task UpdatePhone(ItemPhoneModel phoneModel)
    {
        await _itemPhoneService.Update(phoneModel);
    }

    public async Task UpdatePhones(List<ItemPhoneModel> phones)
    {
        await _itemPhoneService.UpdateRange(phones);
    }
}