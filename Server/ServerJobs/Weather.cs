using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Server.Core.Abstractions;
using Server.Modules.World;

namespace Server.ServerJobs;

public class Weather : IJob
{
    private readonly ILogger<Weather> _logger;
    private readonly WeatherModule _weatherModule;

    public Weather(
        ILogger<Weather> logger,
        WeatherModule weatherModule)
    {
        _logger = logger;
        _weatherModule = weatherModule;
    }

    public async Task OnSave()
    {
        await Task.CompletedTask;
    }

    public async Task OnShutdown()
    {
        await Task.CompletedTask;
    }

    public async Task OnStartup()
    {
        _weatherModule.GetRandomWeather();

        await Task.CompletedTask;
    }
}