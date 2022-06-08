using System.Collections.Generic;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Enums;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Modules.Context;
using Server.Modules.Group;

namespace Server.Handlers.Action;

public class VehicleActionHandler : ISingletonScript
{
    private readonly VehicleCatalogService _vehicleCatalogService;
    private readonly VehicleService _vehicleService;

    private readonly ContextModule _contextModule;
    private readonly GroupModule _groupModule;

    public VehicleActionHandler(
        VehicleCatalogService vehicleCatalogService,
        VehicleService vehicleService,
        ContextModule contextModule,
        GroupModule groupModule)
    {
        _vehicleCatalogService = vehicleCatalogService;
        _vehicleService = vehicleService;

        _contextModule = contextModule;
        _groupModule = groupModule;

        AltAsync.OnClient<ServerPlayer, int>("vehicleactions:get", OnGetActions);
    }

    private async void OnGetActions(ServerPlayer player, int vehicleDbId)
    {
        if (!player.Exists || player.IsInVehicle)
        {
            return;
        }

        var dbVehicle = await _vehicleService.GetByKey(vehicleDbId);
        if (dbVehicle == null)
        {
            return;
        }

        var catalogVehicle = await _vehicleCatalogService.GetByKey(dbVehicle.Model);
        if (catalogVehicle == null)
        {
            return;
        }

        var isInGroup = false;

        if (dbVehicle.GroupModelOwnerId.HasValue)
        {
            isInGroup = await _groupModule.IsPlayerInGroup(player, dbVehicle.GroupModelOwnerId.Value);
        }

        var actions = new List<ActionData>()
        {
            new("Kofferraum öffnen", "vehiclemenu:trunk", vehicleDbId),
            new("Auf- & Abschließen", "vehiclemenu:lock", vehicleDbId),
            new("Letzten Fahrer", "vehiclemenu:requestlastdrivers", vehicleDbId),
            new("Fahrzeug auftanken", "vehiclemenu:requestrefuel", vehicleDbId)
        };

        if (player.CharacterModel.Id == dbVehicle.CharacterModelId || isInGroup)
        {
            actions.Add(new("Fahrzeug verkaufen", "vehiclemenu:sell", vehicleDbId));
        }

        _contextModule.OpenMenu(player, catalogVehicle.DisplayName, actions);
    }
}