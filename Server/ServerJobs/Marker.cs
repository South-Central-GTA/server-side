using System.Numerics;
using System.Threading.Tasks;
using AltV.Net.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Server.Core.Abstractions;
using Server.Core.Configuration;
using Server.Data.Enums.EntitySync;
using Server.DataAccessLayer.Services;
using Server.Modules.EntitySync;

namespace Server.ServerJobs;

public class Marker : IJob
{
    private readonly DevelopmentOptions _developmentOptions;

    private readonly HouseService _houseService;
    private readonly ILogger<DroppedItems> _logger;

    private readonly MarkerSyncModule _markerSyncModule;
    private readonly WorldLocationOptions _worldLocationOptions;

    public Marker(
        ILogger<DroppedItems> logger,
        IOptions<WorldLocationOptions> worldLocationOptions,
        IOptions<DevelopmentOptions> developmentOptions,
        HouseService houseService,
        MarkerSyncModule markerSyncModule)
    {
        _logger = logger;
        _worldLocationOptions = worldLocationOptions.Value;
        _developmentOptions = developmentOptions.Value;

        _houseService = houseService;

        _markerSyncModule = markerSyncModule;
    }

    public async Task OnSave()
    {
        await Task.CompletedTask;
    }

    public async Task OnShutdown()
    {
        _markerSyncModule.DeleteAll();
        await Task.CompletedTask;
    }

    public async Task OnStartup()
    {
        if (_developmentOptions.DropDatabaseAtStartup)
        {
            return;
        }

        _markerSyncModule.Create(MarkerType.VERTICAL_CYLINDER,
                                 new Position(_worldLocationOptions.HarbourSelectionPositionX,
                                              _worldLocationOptions.HarbourSelectionPositionY,
                                              _worldLocationOptions.HarbourSelectionPositionZ),
                                 Vector3.Zero,
                                 Vector3.Zero,
                                 new Vector3(4f, 4f, 1f),
                                 new Rgba(255, 255, 255, 50),
                                 0,
                                 false,
                                 20,
                                 "~y~Hafen Los Santos\n~w~Aufladestation");

        foreach (var publicGarage in _worldLocationOptions.PublicGarages)
        {
            _markerSyncModule.Create(MarkerType.VERTICAL_CYLINDER,
                                     new Position(publicGarage.ParkingPointX,
                                                  publicGarage.ParkingPointY,
                                                  publicGarage.ParkingPointZ),
                                     Vector3.Zero,
                                     Vector3.Zero,
                                     new Vector3(4f, 4f, 1f),
                                     new Rgba(255, 255, 255, 50),
                                     0,
                                     false,
                                     20,
                                     publicGarage.Name);
        }

        foreach (var house in await _houseService.GetAll())
        {
            if (house.BlockedOwnership)
            {
                continue;
            }

            _markerSyncModule.Create(MarkerType.VERTICAL_CYLINDER,
                                     house.Position,
                                     Vector3.Zero,
                                     Vector3.Zero,
                                     new Vector3(0.8f, 0.8f, 1f),
                                     new Rgba(255, 255, 255, 10),
                                     0,
                                     false,
                                     5);
        }

        foreach (var interiorData in _worldLocationOptions.IntPositions)
        {
            _markerSyncModule.Create(MarkerType.VERTICAL_CYLINDER,
                                     new Position(interiorData.X, interiorData.Y, interiorData.Z),
                                     Vector3.Zero,
                                     Vector3.Zero,
                                     new Vector3(0.8f, 0.8f, 1f),
                                     new Rgba(255, 255, 255, 10),
                                     0,
                                     false,
                                     5);
        }

        await Task.CompletedTask;
    }
}