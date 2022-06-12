using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Modules.Vehicles;

namespace Server.Handlers.Vehicle;

public class VehicleEngineHandler : ISingletonScript
{
    private readonly VehicleModule _vehicleModule;

    public VehicleEngineHandler(VehicleModule vehicleModule)
    {
        _vehicleModule = vehicleModule;

        AltAsync.OnClient<ServerPlayer>("vehicle:toggleengine", OnExecute);
        AltAsync.OnClient<ServerPlayer>("invehiclemenu:toggleengine", OnExecute);

    }

    private async void OnExecute(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        await _vehicleModule.SetEngineState((ServerVehicle)player.Vehicle, player, !player.Vehicle.EngineOn);
    }
}