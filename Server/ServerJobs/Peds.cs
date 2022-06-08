using System.Linq;
using System.Threading.Tasks;
using AltV.Net.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Server.Core.Abstractions;
using Server.Core.Configuration;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Housing;
using Server.Modules.EntitySync;

namespace Server.ServerJobs;

public class Peds : IJob
{
    private readonly DevelopmentOptions _devOptions;

    private readonly HouseService _houseService;
    private readonly ILogger<Peds> _logger;

    private readonly PedSyncModule _pedSyncModule;
    private readonly WorldLocationOptions _worldLocationOptions;

    public Peds(
        ILogger<Peds> logger,
        IOptions<DevelopmentOptions> devOptions,
        IOptions<WorldLocationOptions> worldLocationOptions,
        PedSyncModule pedSyncModule,
        HouseService houseService)
    {
        _logger = logger;
        _devOptions = devOptions.Value;
        _worldLocationOptions = worldLocationOptions.Value;

        _pedSyncModule = pedSyncModule;

        _houseService = houseService;
    }

    public async Task OnSave()
    {
        await Task.CompletedTask;
    }

    public async Task OnShutdown()
    {
        _pedSyncModule.DeleteAll();
        await Task.CompletedTask;
    }

    public async Task OnStartup()
    {
        if (_devOptions.DropDatabaseAtStartup)
        {
            return;
        }

        foreach (var publicGarage in _worldLocationOptions.PublicGarages)
        {
            _pedSyncModule.Create(publicGarage.PedModel,
                                  new Position(publicGarage.PedPointX, publicGarage.PedPointY, publicGarage.PedPointZ),
                                  publicGarage.PedHeading,
                                  0);
        }

        foreach (var drivingSchool in _worldLocationOptions.DrivingSchools)
        {
            _pedSyncModule.Create(drivingSchool.PedModel,
                                  new Position(drivingSchool.PedPointX,
                                               drivingSchool.PedPointY,
                                               drivingSchool.PedPointZ),
                                  drivingSchool.PedHeading,
                                  0);
        }

        var houses = await _houseService.GetAll();
        foreach (var leaseCompanyHouse in houses.Where(h => h.HouseType == HouseType.COMPANY)
                                                .Cast<LeaseCompanyHouseModel>())
        {
            _pedSyncModule.CreateCashier(leaseCompanyHouse);
        }

        _pedSyncModule.Create("u_m_y_gunvend_01",
                              new Position(_worldLocationOptions.CityHallPositionX,
                                           _worldLocationOptions.CityHallPositionY,
                                           _worldLocationOptions.CityHallPositionZ),
                              0,
                              0);

        _pedSyncModule.Create("s_m_m_prisguard_01",
                              new Position(349.85934f, -1600.5099f, 28.279907f),
                              -36.85f,
                              0);

        await Task.CompletedTask;
    }
}