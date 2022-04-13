using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Enums;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Database.Enums;
using Server.Modules.Key;
using Server.Modules.Vehicles;

namespace Server.Handlers.Vehicle;

public class VehicleLockHandler : ISingletonScript
{
    private readonly LockModule _lockModule;
    private readonly VehicleModule _vehicleModule;

    public VehicleLockHandler(
        VehicleModule vehicleModule,
        LockModule lockModule)
    {
        _vehicleModule = vehicleModule;
        _lockModule = lockModule;

        AltAsync.OnClient<ServerPlayer, int>("vehiclemenu:lock", OnLockVehicle);
    }

    private async void OnLockVehicle(ServerPlayer player, int vehicleId)
    {
        if (!player.Exists)
        {
            return;
        }

        if (player.IsInVehicle)
        {
            player.SendNotification("Dein Charakter darf in keinem Fahrzeug sitzen.", NotificationType.ERROR);
            return;
        }

        var vehicle = Alt.GetAllVehicles().FindByDbId(vehicleId);
        if (vehicle is not { Exists: true })
        {
            return;
        }

        var lockState = await _lockModule.Lock(player, vehicle.DbEntity);
        if (lockState == null)
        {
            return;
        }

        if (lockState == LockState.OPEN)
        {
            await vehicle.SetLockStateAsync(VehicleLockState.Unlocked);
        }

        if (lockState == LockState.CLOSED)
        {
            await vehicle.SetLockStateAsync(VehicleLockState.Locked);
        }

        await _vehicleModule.SetSyncedDataAsync(vehicle);
    }
}