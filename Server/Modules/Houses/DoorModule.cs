using System.Threading.Tasks;
using AltV.Net.Data;
using Server.Core.Abstractions.ScriptStrategy;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Housing;
using Server.Modules.EntitySync;

namespace Server.Modules.Houses;

public class DoorModule : ISingletonScript
{
    private readonly DoorService _doorService;
    private readonly HouseService _houseService;
    private readonly DoorSyncModule _doorSyncModule;

    public DoorModule(
        HouseService houseService,
        DoorService doorService,
        DoorSyncModule doorSyncModule)
    {
        _houseService = houseService;
        _doorService = doorService;
        _doorSyncModule = doorSyncModule;
    }

    public async Task AddDoor(HouseModel houseModel, uint doorMesh, Position doorPosition)
    {
        houseModel.Doors.Add(await _doorService.Add(new DoorModel(doorMesh, doorPosition, houseModel.Id)));

        _doorSyncModule.Create(doorMesh, doorPosition, 0, houseModel.LockState == LockState.CLOSED, houseModel.Id);
        await _houseService.Update(houseModel);
    }

    public async Task<bool> RemoveDoor(HouseModel houseModel, int doorId)
    {
        var door = await _doorService.GetByKey(doorId);
        if (door == null)
        {
            return false;
        }

        houseModel.Doors.Remove(door);

        await _doorService.Remove(door);
        await _houseService.Update(houseModel);

        return true;
    }
}