using System.Collections.Generic;
using System.Threading.Tasks;
using AltV.Net.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Server.Core.Abstractions;
using Server.Core.Configuration;
using Server.Modules;
using Server.Modules.EntitySync;

namespace Server.ServerJobs;

public class Blip : IServerJob
{
    private readonly ILogger<DroppedItems> _logger;
    private readonly WorldLocationOptions _worldLocationOptions;
    private readonly DevelopmentOptions _developmentOptions;

    private readonly BlipSyncModule _blipSyncModule;

    private readonly List<Position> _atmPositions = new()
    {
        new Position( 129.75f, -1710.50f, 28.52f), // herr kutz
        new Position( 455.47f, -2056.70f, 23.04f), // corner shop ls
        new Position( 795.76f, -2109.49f, 28.47f), // ammo
        new Position( 429.49f, -1514.13f, 28.46f), // legal meetup
        new Position( -245.68f, -1542.97f, 30.72f), // illegal meetup
        new Position( 539.38f, -1582.14f, 28.29f), // water and power ls
        new Position( -652.40f, -1774.93f, 23.65f), // factory
        new Position( -57.40f, -1751.74f, 28.42f), // davis supermarket
        new Position( 33.19432f, -1348.806f, 28.49f), // strawberry supermarket
        new Position( 1077.7583f, -776.4132f, 57.227783f), // mirror park
        new Position( 1153.7803f, -326.7165f, 68.19702f), // mirror park gas station
        new Position( 1138.3385f, -468.94946f, 65.72009f), // mirror park shopping center
        new Position( 129.62637f, -1291.978f, 28.263062f), // ATMVANILLA
        new Position( 161.97363f, -1556.545f, 28.246094f), // ATMRON
        new Position( 383.5121f, -1779.0198f, 28.515747f), // ATMMOTELDAVIS
        new Position( 382.52307f, -1418.8484f, 28.263062f), // ATMKH1
        new Position( 392.30768f, -1426.6945f, 28.263062f), // ATMKH2
        new Position( 305.64395f, -1428.7781f, 28.802246f), // ATMKH3
        new Position( 319.12088f, -1392.6329f, 31.41394f), // ATMKH4
        new Position( 321.2967f, -1394.2021f, 31.41394f), // ATMKH5
        new Position( 283.55603f, -1444.6549f, 28.953857f), // ATMKH6
        new Position( 315.4286f, -1464.2373f, z: 28.953857f), // ATMKH7
    };

    public Blip(
        ILogger<DroppedItems> logger,
        IOptions<WorldLocationOptions> worldLocationOptions,
        IOptions<DevelopmentOptions> developmentOptions,
        BlipSyncModule blipSyncModule)
    {
        _logger = logger;
        _worldLocationOptions = worldLocationOptions.Value;
        _developmentOptions = developmentOptions.Value;

        _blipSyncModule = blipSyncModule;
    }

    public async Task OnSave()
    {
        await Task.CompletedTask;
    }

    public async Task OnShutdown()
    {
        _blipSyncModule.DeleteAll();
        await Task.CompletedTask;
    }

    public async Task OnStartup()
    {
        if (_developmentOptions.DropDatabaseAtStartup)
        {
            return;
        }

        foreach (var atmPosition in _atmPositions)
        {
            _blipSyncModule.Create("Geldautomat", 2, 1, true, 272, 
                                   new Position(atmPosition.X, atmPosition.Y, atmPosition.Z));
        }
        
        _blipSyncModule.Create("Hafen Los Santos Aufladestation", 5, 1, true, 478, 
                               new Position(
                                   _worldLocationOptions.HarbourSelectionPositionX, 
                                   _worldLocationOptions.HarbourSelectionPositionY,
                                   _worldLocationOptions.HarbourSelectionPositionZ));

        foreach (var publicGarage in _worldLocationOptions.PublicGarages)
        {
            _blipSyncModule.Create("Öffentliche Garage", 42, 1, true, 50, 
                                   new Position(
                                       publicGarage.ParkingPointX,
                                       publicGarage.ParkingPointY,
                                       publicGarage.ParkingPointZ));
        }

        foreach (var drivingSchool in _worldLocationOptions.DrivingSchools)
        {
            _blipSyncModule.Create("Fahrschule", 45, 1, true, 498, 
                                   new Position(
                                       drivingSchool.PedPointX,
                                       drivingSchool.PedPointY,
                                       drivingSchool.PedPointZ));
        }

        _blipSyncModule.Create("Stadthalle", 4, 1, true, 120,
                               new Position(_worldLocationOptions.CityHallPositionX,
                                            _worldLocationOptions.CityHallPositionY,
                                            _worldLocationOptions.CityHallPositionZ));

        await Task.CompletedTask;
    }
}