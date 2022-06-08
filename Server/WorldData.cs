using System;
using AltV.Net;
using AltV.Net.Enums;
using Server.Core.Abstractions.ScriptStrategy;

namespace Server;

public class WorldData : ISingletonScript
{
    public WorldData()
    {
        TimeZoneInfo? cetInfo = null;

        try
        {
            cetInfo = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
        }
        catch (Exception e)
        {
            if (e is TimeZoneNotFoundException)
            {
                cetInfo = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
            }
        }

        if (cetInfo == null)
        {
            return;
        }

        var cetTime = TimeZoneInfo.ConvertTime(DateTimeOffset.Now, cetInfo);
        Clock = cetTime.AddHours(cetTime.Hour - 2).DateTime;

        Weather = WeatherType.Clear;
    }

    public DateTime Clock { get; set; }

    public WeatherType Weather
    {
        get
        {
            if (!Alt.GetSyncedMetaData("Weather", out uint weatherId) ||
                !Enum.IsDefined(typeof(WeatherType), weatherId))
            {
                return WeatherType.Clear;
            }

            return (WeatherType)weatherId;
        }
        set => Alt.SetSyncedMetaData("Weather", (uint)value);
    }

    public bool Blackout
    {
        get => Alt.GetSyncedMetaData("Blackout", out bool blackout) && blackout;
        set => Alt.SetSyncedMetaData("Blackout", value);
    }
}