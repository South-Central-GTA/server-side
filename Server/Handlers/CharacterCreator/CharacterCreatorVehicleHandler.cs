using AltV.Net.Async;
using AltV.Net.Data;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Modules.Vehicles;

namespace Server.Handlers.CharacterCreator;

public class CharacterCreatorVehicleHandler : ISingletonScript
{
    private readonly VehicleModule _vehicleModule;
    private readonly VehicleSelectionModule _vehicleSelectorModule;

    public CharacterCreatorVehicleHandler(VehicleModule vehicleModule, VehicleSelectionModule vehicleSelectorModule)
    {
        _vehicleModule = vehicleModule;
        _vehicleSelectorModule = vehicleSelectorModule;

        AltAsync.OnClient<ServerPlayer>("charcreatorvehicle:open", OnRequestOpenVehicleMenu);
    }

    private async void OnRequestOpenVehicleMenu(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        var vehicleCatalog = await _vehicleSelectorModule.GetStarterVehicles();
        foreach (var catalogVehicle in vehicleCatalog)
        {
            catalogVehicle.SouthCentralPoints = await _vehicleModule.GetPointsPrice(catalogVehicle.Model);
        }

        player.EmitLocked("vehicleselector:open", vehicleCatalog);

        player.SetPositionLocked(new Position(181.397f, -1000.905f, -99.014f));
    }
}