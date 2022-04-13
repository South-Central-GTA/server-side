using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Server.Modules;
using Server.Modules.World;

namespace Server.ScheduledJob;

public class WeatherScheduledJob
    : ScheduledJob
{
    private static readonly Random Random = new();
    private readonly ILogger<WeatherScheduledJob> _logger;
    private readonly WeatherModule _weatherModule;

    public WeatherScheduledJob(
        ILogger<WeatherScheduledJob> logger,
        WeatherModule weatherModule)
        : base(TimeSpan.FromMinutes(Random.Next(20, 90)))
    {
        _logger = logger;
        _weatherModule = weatherModule;
    }

    public override async Task Action()
    {
        _weatherModule.GetRandomWeather();
        await Task.CompletedTask;
    }
}