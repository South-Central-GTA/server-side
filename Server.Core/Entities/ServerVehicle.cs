using System;
using System.Collections.Concurrent;
using System.Timers;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Server.Data.Enums;
using Server.Database.Models.Vehicles;

namespace Server.Core.Entities;

public class ServerVehicle
    : Vehicle
{
    public ServerVehicle(ICore core, IntPtr entityPointer, ushort id)
        : base(core, entityPointer, id)
    {
    }

    public ServerVehicle(uint model, Position position, Rotation rotation)
        : base(Alt.Core, model, position, rotation)
    {
    }

    public ConcurrentDictionary<string, int> TempData { get; set; } = new();
    public ConcurrentDictionary<string, Timer> Timers { get; set; } = new();

    public VehicleIndicator ActiveIndicators
    {
        get
        {
            if (!GetStreamSyncedMetaData("indicators", out int result))
            {
                return VehicleIndicator.NONE;
            }

            return (VehicleIndicator)result;
        }
        set => SetStreamSyncedMetaData("indicators", (int)value);
    }

    public PlayerVehicleModel? DbEntity { get; set; }

    public float Fuel { get; set; }
    public float DrivenKilometre { get; set; }
    public Position? LastCheckPosition { get; set; }
}