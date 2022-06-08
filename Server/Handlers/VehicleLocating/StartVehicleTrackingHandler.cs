using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Modules.Vehicles;

namespace Server.Handlers.VehicleLocating;

public class StartVehicleTrackingHandler : ISingletonScript
{
    private readonly VehicleLocatingModule _vehicleLocatingModule;

    public StartVehicleTrackingHandler(VehicleLocatingModule vehicleLocatingModule)
    {
        _vehicleLocatingModule = vehicleLocatingModule;

        AltAsync.OnClient<ServerPlayer, int>("locating:trackvehicle", OnStartTracking);
    }

    private async void OnStartTracking(ServerPlayer player, int vehicleDbId)
    {
        await _vehicleLocatingModule.StartTracking(player, vehicleDbId);
    }
}