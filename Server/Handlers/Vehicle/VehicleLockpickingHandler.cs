using AltV.Net;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;

namespace Server.Handlers.Vehicle;

public class VehicleLockpickingHandler : ISingletonScript
{
    public VehicleLockpickingHandler()
    {
        AltAsync.OnClient<ServerPlayer, int>("vehiclemenu:lockpicking", OnExecute);
    }

    private async void OnExecute(ServerPlayer player, int vehicleDbId)
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

        var vehicle = Alt.GetAllVehicles().FindByDbId(vehicleDbId);
        if (vehicle is not { Exists: true } || vehicle.DbEntity == null)
        {
            return;
        }

        player.EmitLocked("lockpicking:start", vehicle.DbEntity.Id);
    }
}