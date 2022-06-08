using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Modules.Narrator;

namespace Server.Handlers.Weapon;

public class SerialNumberHandler : ISingletonScript
{
    private readonly ItemWeaponService _itemWeaponService;

    private readonly NarratorModule _narratorModule;

    public SerialNumberHandler(
        ItemWeaponService itemWeaponService,
        NarratorModule narratorModule)
    {
        _itemWeaponService = itemWeaponService;
        _narratorModule = narratorModule;

        AltAsync.OnClient<ServerPlayer, int>("serialnumber:show", OnShow);
        AltAsync.OnClient<ServerPlayer, int>("serialnumber:requestremove", OnRequestRemove);
        AltAsync.OnClient<ServerPlayer, int>("serialnumber:remove", OnRemove);
    }

    private async void OnShow(ServerPlayer player, int itemId)
    {
        if (!player.Exists)
        {
            return;
        }

        var item = await _itemWeaponService.GetByKey(itemId);

        if (player.CharacterModel.InventoryModel.Id != item?.InventoryModelId)
        {
            return;
        }

        if (string.IsNullOrEmpty(item.SerialNumber))
        {
            _narratorModule.SendMessage(player, $"Die Seriennummer ist nicht mehr erkennbar.");
        }
        else
        {
            _narratorModule.SendMessage(player, $"Die Seriennummer lautet {item.SerialNumber}.");
        }
    }

    private async void OnRequestRemove(ServerPlayer player, int itemId)
    {
        if (!player.Exists)
        {
            return;
        }

        var item = await _itemWeaponService.GetByKey(itemId);

        if (player.CharacterModel.InventoryModel.Id != item?.InventoryModelId)
        {
            return;
        }

        if (string.IsNullOrEmpty(item.SerialNumber))
        {
            player.SendNotification("Die Seriennummer ist schon entfernt", NotificationType.ERROR);
            return;
        }

        var data = new object[1];
        data[0] = itemId;

        player.CreateDialog(new DialogData
        {
            Type = DialogType.TWO_BUTTON_DIALOG,
            Title = "Seriennummer entfernen",
            Description =
                $"Bist du sicher das du die Seriennummer von deiner Waffe entfernen möchtest?<br><br><span class='text-muted'>Der Besitz dieser Waffe ist dann illegal und es kann nicht rückgängig gemacht werden!</span>",
            HasBankAccountSelection = false,
            FreezeGameControls = true,
            Data = data,
            PrimaryButton = "Ja",
            PrimaryButtonServerEvent = "serialnumber:remove",
            SecondaryButton = "Nein"
        });
    }

    private async void OnRemove(ServerPlayer player, int itemId)
    {
        if (!player.Exists)
        {
            return;
        }

        var item = await _itemWeaponService.GetByKey(itemId);

        if (player.CharacterModel.InventoryModel.Id != item?.InventoryModelId)
        {
            return;
        }

        if (string.IsNullOrEmpty(item.SerialNumber))
        {
            player.SendNotification("Die Seriennummer ist schon entfernt", NotificationType.ERROR);
            return;
        }

        item.SerialNumber = "";

        await _itemWeaponService.Update(item);

        player.SendNotification("Die Seriennummer wurde entfernt", NotificationType.SUCCESS);
    }
}