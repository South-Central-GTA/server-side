using System.Collections.Generic;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Data.Models;
using Server.Modules.Context;
using Server.Modules.Vehicles;

namespace Server.Handlers.Action;

public class InVehicleActionHandler : ISingletonScript
{
    private readonly ContextModule _contextModule;
    private readonly VehicleModule _vehicleModule;

    public InVehicleActionHandler(ContextModule contextModule, VehicleModule vehicleModule)
    {
        _contextModule = contextModule;
        _vehicleModule = vehicleModule;
        
        AltAsync.OnClient<ServerPlayer>("invehicleactions:get", OnGetActions);
    }

    private async void OnGetActions(ServerPlayer player)
    {
        if (!player.Exists || !player.IsInVehicle)
        {
            return;
        }

        if (!_vehicleModule.HasEngine(player.Vehicle))
        {
            return;
        }

        var toggledEngineStatus = player.Vehicle.EngineOn ? "ausschalten" : "einschalten";

        var actions = new List<ActionData>
        {
            new($"Motor { toggledEngineStatus }", "invehiclemenu:toggleengine"),
            new("Fahrzeug kurzschlißen", "invehiclemenu:shortcircuit")
        };

        _contextModule.OpenMenu(player, "Fahrzeug", actions);
    }
}