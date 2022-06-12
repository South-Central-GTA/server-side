using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Modules;
using Server.Modules.Vehicles;

namespace Server.Handlers.Vehicle;

public class VehicleShortCircuitHandler : ISingletonScript
{
    private readonly ShortingModule _shortingModule;

    public VehicleShortCircuitHandler(ShortingModule shortingModule)
    {
        _shortingModule = shortingModule;

        AltAsync.OnClient<ServerPlayer>("invehiclemenu:shortcircuit", OnExecute);
    }

    private async void OnExecute(ServerPlayer player)
    {
        if (!player.Exists || !player.IsInVehicle)
        {
            return;
        }

        await _shortingModule.ShortCircuitAsync(player, (ServerVehicle)player.Vehicle);
    }
}