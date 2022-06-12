using System.Collections.Generic;
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
using Server.Modules.Context;
using Server.Modules.Group;

namespace Server.Handlers.Action;

public class VehicleActionHandler : ISingletonScript
{
    private readonly ContextModule _contextModule;
    private readonly GroupModule _groupModule;
    private readonly VehicleCatalogService _vehicleCatalogService;
    private readonly VehicleService _vehicleService;

    public VehicleActionHandler(VehicleCatalogService vehicleCatalogService, VehicleService vehicleService,
        ContextModule contextModule, GroupModule groupModule)
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

        var toggledDoorState = dbVehicle.LockState == LockState.OPEN ? "Abschließen" : "Aufschließen";

        var actions = new List<ActionData>
        {
            new(toggledDoorState, "vehiclemenu:lock", vehicleDbId),
            new("Letzten Fahrer", "vehiclemenu:requestlastdrivers", vehicleDbId),
        };

        if (dbVehicle.LockState == LockState.OPEN)
        {
            actions.Add(new ActionData("Kofferraum öffnen", "vehiclemenu:trunk", vehicleDbId));
            actions.Add(new ActionData("Fahrzeug auftanken", "vehiclemenu:requestrefuel", vehicleDbId));
        }
        
        if (player.CharacterModel.Id == dbVehicle.CharacterModelId || isInGroup)
        {
            actions.Add(new ActionData("Fahrzeug verkaufen", "vehiclemenu:sell", vehicleDbId));
        }
        
        if (dbVehicle.LockState == LockState.CLOSED 
            && player.CharacterModel.InventoryModel.Items.Any(i => i.CatalogItemModelId == ItemCatalogIds.LOCKPICK))
        {
            actions.Add(new ActionData("Schloss aufknacken", "vehiclemenu:lockpicking", vehicleDbId));
        }

        _contextModule.OpenMenu(player, catalogVehicle.DisplayName, actions);
    }
}