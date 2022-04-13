using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Modules.Vehicles;

namespace Server.Handlers.VehicleLocating;

public class StopVehicleTrackingHandler : ISingletonScript
{
    private readonly VehicleLocatingModule _vehicleLocatingModule;
    
    public StopVehicleTrackingHandler(VehicleLocatingModule vehicleLocatingModule)
    {
        _vehicleLocatingModule = vehicleLocatingModule;
        
        AltAsync.OnClient<ServerPlayer>("locating:stop", OnStop);
    }

    private void OnStop(ServerPlayer player)
    {
        _vehicleLocatingModule.StopTracking(player);
    }
}