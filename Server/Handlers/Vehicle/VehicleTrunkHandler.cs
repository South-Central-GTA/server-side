using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Enums;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Database.Enums;
using Server.Modules.Dump;

namespace Server.Handlers.Vehicle;

public class VehicleTrunkHandler : ISingletonScript
{
    private readonly VehicleDumpModule _vehicleDumpModule;

    public VehicleTrunkHandler(VehicleDumpModule vehicleDumpModule)
    {
        _vehicleDumpModule = vehicleDumpModule;

        AltAsync.OnClient<ServerPlayer, int>("vehiclemenu:trunk", OnOpenTrunk);
        AltAsync.OnClient<ServerPlayer>("vehicleinventory:close", OnCloseTrunk);
    }

    private void OnOpenTrunk(ServerPlayer player, int vehicleDbId)
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
        if (vehicle is not { Exists: true })
        {
            return;
        }

        if (player.Position.Distance(vehicle.Position) > 6)
        {
            return;
        }

        if (vehicle.DbEntity.VehicleState == VehicleState.DESTROYED)
        {
            player.SendNotification("Dein Charakter bekommt den Kofferraum nicht mehr geöffnet.",
                                    NotificationType.ERROR);
            return;
        }

        if (vehicle.LockState == VehicleLockState.Locked)
        {
            player.SendNotification("Der Kofferraum ist verschlossen.", NotificationType.ERROR);
            return;
        }

        var dumpEntry = _vehicleDumpModule.Dump.Find(vd => vd.Hash == vehicle.Model);
        if (dumpEntry is { Class: "CYCLE" })
        {
            player.SendNotification("Fahrräder haben keine Kofferräume oder sonstige Lagerpositionen.",
                                    NotificationType.ERROR);
            return;
        }

        player.SetData("INTERACT_VEHICLE_TRUNK", vehicleDbId);

        player.EmitLocked("vehicleinventory:interact", vehicle);
    }

    private void OnCloseTrunk(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        player.DeleteData("INTERACT_VEHICLE_TRUNK");
    }
}