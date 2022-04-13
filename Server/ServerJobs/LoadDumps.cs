using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Server.Core.Abstractions;
using Server.Data.Dumps;
using Server.Helper;
using Server.Modules;
using Server.Modules.Dump;

namespace Server.ServerJobs;

public class LoadDumps : IServerJob
{
    private readonly ILogger<LoadDumps> _logger;
    private readonly Serializer _serializer;
    private readonly VehicleDumpModule _vehicleDumpModule;

    public LoadDumps(
        ILogger<LoadDumps> logger,
        VehicleDumpModule vehicleDumpModule,
        Serializer serializer)
    {
        _logger = logger;
        _vehicleDumpModule = vehicleDumpModule;
        _serializer = serializer;
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
        _logger.LogInformation("Try to load all required dump informations.");

        var vehicleJson = await File.ReadAllTextAsync(Path.Combine(Directory.GetCurrentDirectory(), "resources", "southcentral-assets", "dumps", "vehicles.json"));
        _vehicleDumpModule.SetData(_serializer.Deserialize<List<VehicleDumpEntry>>(vehicleJson));
        _logger.LogInformation($"Loaded {_vehicleDumpModule.Dump.Count} vehicle dump informations.");

        await Task.CompletedTask;
    }
}