using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Server.Core.Abstractions;
using Server.Core.Configuration;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Housing;
using Server.Modules.EntitySync;
using Server.Modules.Houses;

namespace Server.ServerJobs;

public class HouseDatabase : IJob
{
    private readonly DevelopmentOptions _devOptions;
    private readonly DoorSyncModule _doorSyncModule;
    private readonly HouseModule _houseModule;
    private readonly HouseService _houseService;
    private readonly ILogger<HouseDatabase> _logger;

    public HouseDatabase(ILogger<HouseDatabase> logger, HouseService houseService,
        IOptions<DevelopmentOptions> devOptions, HouseModule houseModule, DoorSyncModule doorSyncModule)
    {
        _logger = logger;
        _houseService = houseService;
        _devOptions = devOptions.Value;
        _houseModule = houseModule;
        _doorSyncModule = doorSyncModule;
    }

    public async Task OnSave()
    {
        await Task.CompletedTask;
    }

    public async Task OnShutdown()
    {
        var houses = await _houseService.GetAll();
        var leaseCompanyHouses =
            houses.Where(h => h.HouseType == HouseType.COMPANY).Cast<LeaseCompanyHouseModel>().ToList();

        foreach (var leaseCompanyHouse in leaseCompanyHouses)
        {
            leaseCompanyHouse.PlayerDuties = 0;
        }

        await _houseService.UpdateRange(leaseCompanyHouses);

        await Task.CompletedTask;
    }

    public async Task OnStartup()
    {
        if (!_devOptions.DropDatabaseAtStartup)
        {
            var houses = await _houseService.GetAll();

            await _houseModule.CreateCollShapes();
            await _houseModule.CreateInventories();

            // Create all doors for door entity sync.
            foreach (var house in houses)
            {
                foreach (var houseDoor in house.Doors)
                {
                    _doorSyncModule.Create(houseDoor.Hash, houseDoor.Position, 0, houseDoor.LockState == LockState.CLOSED,
                        house.Id);
                }
            }
        }

        await Task.CompletedTask;
    }
}