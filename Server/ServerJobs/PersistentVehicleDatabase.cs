using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Server.Core.Abstractions;
using Server.Core.Configuration;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Modules.Vehicles;

namespace Server.ServerJobs;

public class PersistentVehicleDatabase : IJob
{
    private readonly DevelopmentOptions _devOptions;
    private readonly ILogger<PersistentVehicleDatabase> _logger;
    private readonly VehicleModule _vehicleModule;
    private readonly VehicleService _vehicleService;

    public PersistentVehicleDatabase(
        ILogger<PersistentVehicleDatabase> logger,
        VehicleModule vehicleModule,
        VehicleService vehicleService,
        IOptions<DevelopmentOptions> devOptions)
    {
        _logger = logger;
        _vehicleModule = vehicleModule;
        _vehicleService = vehicleService;
        _devOptions = devOptions.Value;
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
        if (!_devOptions.DropDatabaseAtStartup)
        {
            var vehicles = await _vehicleService.Where(v => v.VehicleState == VehicleState.SPAWNED);
            foreach (var vehicle in vehicles)
            {
                await _vehicleModule.Create(vehicle);
            }
        }

        await Task.CompletedTask;
    }
}