using System.Collections.Concurrent;
using System.Timers;
using AltV.Net.Async;
using Server.Core.Entities;

namespace Server.Core.Extensions;

public static class ServerVehicleExtensions
{
    private static ConcurrentDictionary<string, Timer> _timers = new();

    public static void SetRepairValue(this ServerVehicle vehicle, int value = 1000, bool fixCosmetics = true)
    {
        vehicle.SetBodyHealthAsync((uint)value);
        vehicle.EngineHealth = value;

        if (!vehicle.NetworkOwner.Exists)
        {
            vehicle.NetworkOwner?.EmitLocked("vehicle:repair", vehicle, vehicle.DbEntity.Id, value, fixCosmetics);
        }
    }

    public static void CreateTimer(this ServerVehicle vehicle, string id, ElapsedEventHandler callback,
        int milliseconds)
    {
        var timer = new Timer { Interval = milliseconds, AutoReset = false, Enabled = true };
        if (vehicle.Timers.TryAdd(id, timer))
        {
            timer.Elapsed += callback;
            timer.Start();
        }
    }

    public static void ClearTimer(this ServerVehicle vehicle, string id)
    {
        if (vehicle.Timers.TryRemove(id, out var timer))
        {
            timer.Dispose();
        }
    }

    public static void ClearAllTimer(this ServerVehicle vehicle)
    {
        foreach (var t in vehicle.Timers.Keys)
        {
            if (vehicle.Timers.TryRemove(t, out var timer))
            {
                timer.Dispose();
            }
        }
    }
}