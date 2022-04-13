using System;
using System.Linq;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Inventory;
using Server.Helper;
using Server.Modules.Inventory;

namespace Server.Handlers.Items;

public class ItemRadioHandler : ISingletonScript
{
    private readonly InventoryModule _inventoryModule;

    private readonly InventoryService _inventoryService;
    private readonly ItemService _itemService;
    private readonly Serializer _serializer;

    public ItemRadioHandler(
        Serializer serializer,
        ItemService itemService,
        InventoryService inventoryService,
        InventoryModule inventoryModule)
    {
        _serializer = serializer;

        _itemService = itemService;
        _inventoryService = inventoryService;

        _inventoryModule = inventoryModule;

        AltAsync.OnClient<ServerPlayer, int>("radio:openrequest", OnRequestMenu);
        AltAsync.OnClient<ServerPlayer, string, int>("radio:setfrequency", OnSetFrequency);
    }

    private async void OnRequestMenu(ServerPlayer player, int itemId)
    {
        if (!player.Exists)
        {
            return;
        }

        var item = (ItemRadioModel)await _itemService.GetByKey(itemId);
        if (item == null || player.CharacterModel.InventoryModel.Items.All(i => i.Id != item.Id))
        {
            player.SendNotification("Dein Charakter hat kein Funkgerät im Inventar.", NotificationType.ERROR);
            return;
        }

        var data = new object[1];
        data[0] = itemId;

        switch (item.FactionType)
        {
            case FactionType.CITIZEN:
            {
                player.CreateDialog(new DialogData
                {
                    Type = DialogType.ONE_BUTTON_DIALOG,
                    Title = "Frequenz setzen",
                    Description = "Stelle eine Frequenz ein welche du für das Funkgerät nutzen möchtest. Dies ist ein zivilistes Funkgerät die Frequenz ist zwischen 0-500 einstellbar.",
                    HasInputField = true,
                    FreezeGameControls = true,
                    Data = data,
                    PrimaryButton = "Einstellen",
                    PrimaryButtonServerEvent = "radio:setfrequency"
                });
            }
                break;
            case FactionType.POLICE_DEPARTMENT:
            case FactionType.FIRE_DEPARTMENT:
            {
                player.CreateDialog(new DialogData
                {
                    Type = DialogType.ONE_BUTTON_DIALOG,
                    Title = "Frequenz setzen",
                    Description = "Stelle eine Frequenz ein welche du für das Funkgerät nutzen möchtest. Dies ist ein staatliches Funkgerät die Frequenz ist zwischen 0-5 einstellbar.",
                    HasInputField = true,
                    FreezeGameControls = true,
                    Data = data,
                    PrimaryButton = "Einstellen",
                    PrimaryButtonServerEvent = "radio:setfrequency"
                });
            }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private async void OnSetFrequency(ServerPlayer player, string expectedFrequency, int itemId)
    {
        if (!player.Exists)
        {
            return;
        }

        var item = (ItemRadioModel)await _itemService.GetByKey(itemId);
        if (item == null || player.CharacterModel.InventoryModel.Items.All(i => i.Id != item.Id))
        {
            player.SendNotification("Dein Charakter hat kein Funkgerät im Inventar.", NotificationType.ERROR);
            return;
        }

        if (!int.TryParse(expectedFrequency, out var frequency))
        {
            player.SendNotification("Bitte gebe eine Zahl als Frequenz an.", NotificationType.ERROR);
            return;
        }

        if (frequency < 0)
        {
            player.SendNotification("Bitte gebe eine positive Zahl an.", NotificationType.ERROR);
            return;
        }

        if (frequency > 500 && item.FactionType == FactionType.CITIZEN)
        {
            player.SendNotification("Bitte gebe eine kleinere Zahl als 500 an.", NotificationType.ERROR);
            return;
        }

        if (frequency > 5 && item.FactionType == FactionType.POLICE_DEPARTMENT
            || frequency > 5 && item.FactionType == FactionType.FIRE_DEPARTMENT)
        {
            player.SendNotification("Bitte gebe eine kleinere Zahl als 5 an.", NotificationType.ERROR);
            return;
        }

        item.Frequency = frequency;

        await _itemService.Update(item);
        await _inventoryModule.UpdateInventoryUiAsync(player);

        player.SendNotification("Die Frequenz wurde eingestellt.", NotificationType.SUCCESS);
    }
}