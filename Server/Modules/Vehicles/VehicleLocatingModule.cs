using System;
using System.Threading.Tasks;
using AltV.Net.Data;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Modules.EntitySync;

namespace Server.Modules.Vehicles;

public class VehicleLocatingModule : ISingletonScript
{
    private const int RandomOffset = 100;
    private readonly BlipSyncModule _blipSyncModule;

    private readonly Random _random = new();
    private readonly VehicleService _vehicleService;

    public VehicleLocatingModule(VehicleService vehicleService, BlipSyncModule blipSyncModule)
    {
        _vehicleService = vehicleService;
        _blipSyncModule = blipSyncModule;
    }

    public async Task StartTracking(ServerPlayer player, int vehicleDbId)
    {
        if (!player.Exists)
        {
            return;
        }

        var vehicle = await _vehicleService.GetByKey(vehicleDbId);
        if (vehicle == null)
        {
            return;
        }

        var position = new Position(vehicle.Position.X + _random.Next(-RandomOffset, RandomOffset),
            vehicle.Position.Y + _random.Next(-RandomOffset, RandomOffset), vehicle.Position.Z);

        var serverBlip = _blipSyncModule.Create("Fahrzeugmarkierung", 5, 1, false, 1, position, 0, BlipType.RADIUS,
            player, 150, 40);
        player.SetData("LOCATING_BLIP_ID", serverBlip.Id);

        player.CreateTimer("update_tracking",
            (sender, args) =>
            {
                serverBlip.Position = new Position(vehicle.Position.X + _random.Next(-RandomOffset, RandomOffset),
                    vehicle.Position.Y + _random.Next(-RandomOffset, RandomOffset), vehicle.Position.Z);
            }, 1000 * 10, true);

        player.SendNotification("Tracking von Fahrzeug gestartet.", NotificationType.SUCCESS);
    }

    public void StopTracking(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!player.GetData("LOCATING_BLIP_ID", out ulong blipId))
        {
            return;
        }

        _blipSyncModule.Delete(blipId);

        player.DeleteData("LOCATING_BLIP_ID");
        player.ClearTimer("update_tracking");
        player.SendNotification("Tracking von Fahrzeug beendet.", NotificationType.SUCCESS);
    }
}