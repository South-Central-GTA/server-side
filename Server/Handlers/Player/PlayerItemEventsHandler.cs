﻿using System;
using AltV.Net;
using AltV.Net.Async;
using Microsoft.Extensions.Options;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Configuration;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models._Base;
using Server.Database.Models.Group;
using Server.Database.Models.Housing;
using Server.Database.Models.Inventory;
using Server.Database.Models.Vehicles;
using Server.Modules.Group;
using Server.Modules.Inventory;

namespace Server.Handlers.Player;

public class PlayerItemEventsHandler : ISingletonScript
{
    private readonly GroupModule _groupModule;
    private readonly GroupService _groupService;
    private readonly HouseService _houseService;

    private readonly InventoryModule _inventoryModule;
    private readonly ItemCreationModule _itemCreationModule;
    private readonly ItemKeyService _itemKeyService;
    private readonly ItemService _itemService;
    private readonly Random _random = new();
    private readonly VehicleOptions _vehicleOptions;
    private readonly VehicleService _vehicleService;

    public PlayerItemEventsHandler(IOptions<VehicleOptions> vehicleOptions, HouseService houseService,
        VehicleService vehicleService, GroupService groupService, ItemKeyService itemKeyService,
        ItemService itemService, InventoryModule inventoryModule, GroupModule groupModule,
        ItemCreationModule itemCreationModule)
    {
        _vehicleOptions = vehicleOptions.Value;

        _houseService = houseService;
        _vehicleService = vehicleService;
        _groupService = groupService;
        _itemKeyService = itemKeyService;
        _itemService = itemService;

        _inventoryModule = inventoryModule;
        _groupModule = groupModule;
        _itemCreationModule = itemCreationModule;

        AltAsync.OnClient<ServerPlayer, int>("key:createcopy", OnKeyCreateCopy);

        AltAsync.OnClient<ServerPlayer, int>("repairkit:requestuse", OnRepairKitUse);
        AltAsync.OnClient<ServerPlayer, int, int, int>("repairdialog:repairdialogsuccess", OnRepairDialogSuccess);
    }

    private async void OnKeyCreateCopy(ServerPlayer player, int itemId)
    {
        if (!player.Exists)
        {
            return;
        }

        var itemKey = (ItemKeyModel)await _itemKeyService.GetByKey(itemId);
        if (itemKey == null)
        {
            return;
        }

        if (player.CharacterModel.InventoryModel.Id != itemKey.InventoryModelId)
        {
            return;
        }

        ILockableEntity lockableEntity = null;

        if (itemKey.HouseModelId.HasValue)
        {
            var house = await _houseService.GetByKey(itemKey.HouseModelId);
            if (house == null || house.CharacterModelId != player.CharacterModel.Id)
            {
                return;
            }

            lockableEntity = house;
        }
        else if (itemKey.PlayerVehicleModelId.HasValue)
        {
            var vehicle = await _vehicleService.GetByKey(itemKey.PlayerVehicleModelId);
            if (vehicle == null || vehicle.CharacterModelId != player.CharacterModel.Id)
            {
                return;
            }

            lockableEntity = vehicle;
        }

        if (lockableEntity == null)
        {
            player.SendNotification("Du bist nicht der Eigentümer des Objektes zu welchem der Schlüssel passt.",
                NotificationType.ERROR);
            return;
        }

        var newKey = (ItemKeyModel)await _itemCreationModule.AddItemAsync(player, ItemCatalogIds.KEY, 1, null,
            "Kopie von " + itemKey.Note);

        if (newKey == null)
        {
            player.SendNotification("Dein Charakter hat nicht genug Platz im Inventar.", NotificationType.ERROR);
            return;
        }

        switch (lockableEntity)
        {
            case HouseModel house:
                newKey.HouseModelId = house.Id;
                house.Keys.Add(newKey.Id);
                await _houseService.Update(house);
                break;
            case PlayerVehicleModel vehicle:
                newKey.PlayerVehicleModelId = vehicle.Id;
                vehicle.Keys.Add(newKey.Id);
                await _vehicleService.Update(vehicle);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        await _inventoryModule.UpdateInventoryUiAsync(player);
        player.SendNotification("Du hast erfolgreich eine Kopie des Schlüssels erstellt.", NotificationType.SUCCESS);
    }

    private async void OnRepairKitUse(ServerPlayer player, int itemId)
    {
        if (!player.Exists)
        {
            return;
        }

        var repairKitItem = await _itemService.GetByKey(itemId);
        if (repairKitItem == null)
        {
            return;
        }

        if (player.CharacterModel.InventoryModel.Id != repairKitItem.InventoryModelId)
        {
            return;
        }

        if (!player.IsInVehicle)
        {
            player.SendNotification("Dein Charakter muss in einem Fahrzeug sitzen.", NotificationType.ERROR);
            return;
        }

        var vehicle = (ServerVehicle)player.Vehicle;
        if (vehicle is not { Exists: true } || vehicle.DbEntity == null)
        {
            player.SendNotification("Dieses Fahrzeug kannst du nicht reparieren.", NotificationType.ERROR);
            return;
        }

        if (vehicle.BodyHealth >= 600 && vehicle.EngineHealth >= 600)
        {
            player.SendNotification(
                "Dein Charakter kann das Fahrzeug nicht mit den aktuellen Begebenheiten weiter reparieren.",
                NotificationType.ERROR);
            return;
        }

        vehicle.SetRepairValue(600, false);

        if (_random.NextDouble() <= 0.5) // 50/50 chance 
        {
            await _itemService.Remove(repairKitItem);
            await _inventoryModule.UpdateInventoryUiAsync(player);

            player.SendNotification("Bei der Reperatur ging leider das Werkzeug kaputt.", NotificationType.WARNING);
        }

        player.SendNotification("Du hast das Fahrzeug notdürftig repariert, suche dennoch lieber einen Mechaniker auf.",
            NotificationType.SUCCESS);
    }

    private async void OnRepairDialogSuccess(ServerPlayer player, int groupId, int neededProducts, int vehicleDbId)
    {
        if (!player.Exists)
        {
            return;
        }

        var house = await _houseService.GetByDistance(player.Position, 20);
        if (house?.GroupModelId == null)
        {
            return;
        }

        var group = await _groupService.GetByKey(groupId);
        if (group == null)
        {
            return;
        }

        if (house.GroupModelId.Value != group.Id)
        {
            return;
        }

        var vehicle = Alt.GetAllVehicles().FindByDbId(vehicleDbId);
        if (vehicle is not { Exists: true } || vehicle.DbEntity == null)
        {
            return;
        }

        var companyGroup = (CompanyGroupModel)group;

        if (companyGroup.Products < neededProducts)
        {
            player.SendNotification("Das Unternehmen hat nicht genug Produkte.", NotificationType.ERROR);
            return;
        }

        // Move after dialog:
        companyGroup.Products -= neededProducts;
        await _groupService.Update(companyGroup);
        await _groupModule.UpdateGroupUi(companyGroup);

        vehicle.SetRepairValue();
        player.SendNotification("Du hast das Fahrzeug erfolgreich repariert, spiele die Reperatur nun im Roleplay aus.",
            NotificationType.SUCCESS);
    }
}