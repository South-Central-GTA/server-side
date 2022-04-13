using System.Collections.Generic;
using AltV.Net;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Inventory;
using Server.Modules.Inventory;

namespace Server.Handlers.Frisk;

public class FriskHandler : ISingletonScript
{
    private readonly CharacterService _characterService;

    private readonly InventoryModule _inventoryModule;
    private readonly InventoryService _inventoryService;

    public FriskHandler(
        CharacterService characterService,
        InventoryService inventoryService,
        InventoryModule inventoryModule)
    {
        _characterService = characterService;
        _inventoryService = inventoryService;

        _inventoryModule = inventoryModule;

        AltAsync.OnClient<ServerPlayer, int>("frisk:requestsearch", OnRequestMenu);
        AltAsync.OnClient<ServerPlayer, int, int>("frisk:permissiongranted", OnPermissionGranted);
        AltAsync.OnClient<ServerPlayer, int, int>("frisk:permissiondenied", OnPermissionDenied);
    }

    private async void OnRequestMenu(ServerPlayer player, int playerId)
    {
        if (!player.Exists)
        {
            return;
        }

        var targetPlayer = Alt.GetAllPlayers().FindPlayerById(playerId);
        if (targetPlayer == null)
        {
            player.SendNotification("Spieler konnte nicht gefunden werden.", NotificationType.ERROR);
            return;
        }

        var character = await _characterService.GetByKey(targetPlayer.CharacterModel.Id);

        if (targetPlayer.CharacterModel.DeathState == DeathState.ALIVE)
        {
            var data = new object[2];
            data[0] = character.InventoryModel.Id;
            data[1] = player.Id;

            targetPlayer.CreateDialog(new DialogData
            {
                Type = DialogType.TWO_BUTTON_DIALOG,
                Title = "Durchsuchen",
                Description =
                    $"Der Charakter {player.CharacterModel.Name} möchte deinen Charakter durchsuchen, erlaubst du es?<br><p class='text-muted'>Der Spieler kann frei auf dein Inventar zugreifen.</p>",
                FreezeGameControls = true,
                Data = data,
                PrimaryButton = "Ja",
                PrimaryButtonServerEvent = "frisk:permissiongranted",
                SecondaryButton = "Nein",
                SecondaryButtonServerEvent = "frisk:permissiondenied"
            });
        }
        else
        {
            player.DefaultInventories = new List<InventoryModel> { character.InventoryModel };
            character.InventoryModel.InventoryType = InventoryType.FRISK;

            await _inventoryModule.OpenInventoryUiAsync(player);
        }
    }

    private async void OnPermissionGranted(ServerPlayer player, int inventoryId, int requestPlayerId)
    {
        if (!player.Exists)
        {
            return;
        }

        var inventory = await _inventoryService.GetByKey(inventoryId);
        if (inventory == null)
        {
            return;
        }

        var requestPlayer = Alt.GetAllPlayers().FindPlayerById(requestPlayerId);
        if (requestPlayer == null)
        {
            player.SendNotification("Der andere Spieler konnte nicht mehr gefunden werden.", NotificationType.ERROR);
            return;
        }

        requestPlayer.SendNotification("Der andere Spieler hat deiner Anfrage zugestimmt.", NotificationType.INFO);

        inventory.InventoryType = InventoryType.FRISK;
        requestPlayer.DefaultInventories = new List<InventoryModel> { inventory };

        await _inventoryModule.OpenInventoryUiAsync(requestPlayer);
    }

    private async void OnPermissionDenied(ServerPlayer player, int inventoryId, int requestPlayerId)
    {
        if (!player.Exists)
        {
            return;
        }

        var requestPlayer = Alt.GetAllPlayers().FindPlayerById(requestPlayerId);
        if (requestPlayer == null)
        {
            player.SendNotification("Der andere Spieler konnte nicht mehr gefunden werden.", NotificationType.ERROR);
            return;
        }

        requestPlayer.SendNotification("Der andere Spieler hat deine Abfrage abgelehnt.", NotificationType.INFO);
    }
}