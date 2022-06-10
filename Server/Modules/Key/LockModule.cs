using System.Collections.Generic;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Enums;
using Microsoft.Extensions.Logging;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models._Base;
using Server.Database.Models.Housing;
using Server.Database.Models.Vehicles;
using Server.Modules.EntitySync;
using Server.Modules.Vehicles;

namespace Server.Modules.Key;

public class LockModule : ITransientScript
{
    private readonly DoorService _doorService;
    private readonly DoorSyncModule _doorSyncModule;
    private readonly GroupService _groupService;
    private readonly HouseService _houseService;
    private readonly KeyModule _keyModule;
    private readonly ILogger<LockModule> _logger;
    private readonly VehicleModule _vehicleModule;
    private readonly VehicleService _vehicleService;

    public LockModule(ILogger<LockModule> logger, GroupService groupService, HouseService houseService,
        VehicleService vehicleService, DoorService doorService, DoorSyncModule doorSyncModule,
        VehicleModule vehicleModule, KeyModule keyModule)
    {
        _logger = logger;
        _groupService = groupService;
        _houseService = houseService;
        _vehicleService = vehicleService;
        _doorService = doorService;
        _doorSyncModule = doorSyncModule;
        _vehicleModule = vehicleModule;
        _keyModule = keyModule;
    }

    public async Task<ILockableEntity?> GetClosestLockableEntity(ServerPlayer player, int maxDistance = 2)
    {
        var lockableEntities = new List<ILockableEntity>();

        var vehicle = await _vehicleService.GetByDistance(player.Position, maxDistance);
        if (vehicle != null)
        {
            lockableEntities.Add(vehicle);
        }

        var house = player.Dimension == 0
            ? await _houseService.GetByDistance(player.Position, maxDistance)
            : await _houseService.GetByKey(player.Dimension);
        if (house?.InteriorId != null)
        {
            lockableEntities.Add(house);
        }

        var door = await _doorService.GetByDistance(player.Position, maxDistance);
        if (door != null)
        {
            lockableEntities.Add(door);
        }

        var closestDistance = float.MaxValue;
        ILockableEntity lockableEntity = null;
        foreach (var entity in lockableEntities)
        {
            var distance = new Position(entity.PositionX, entity.PositionY, entity.PositionZ).Distance(player.Position);
            if (distance <= maxDistance && distance < closestDistance)
            {
                closestDistance = distance;
                lockableEntity = entity;
            }
        }

        return lockableEntity;
    }

    public async Task<LockState?> Lock(ServerPlayer player, ILockableEntity lockableEntity, bool force = false)
    {
        if (lockableEntity.LockState == LockState.BROKEN)
        {
            player.SendNotification("Dieses Schloss ist kaputt.", NotificationType.WARNING);
            return null;
        }

        if (!force)
        {
            switch (await _keyModule.HasKey(player, lockableEntity))
            {
                case HasKeyErrorType.HAS_NO_KEY:
                    player.SendNotification("Dein Charakter hat keinen Schlüssel für dieses Schloss.",
                        NotificationType.ERROR);
                    return null;
                case HasKeyErrorType.HAS_WRONG_GROUP_KEY:
                    player.SendNotification(
                        "Der biometrisch gesicherte Schlüssel lässt sich im Schloss nicht umdrehen.",
                        NotificationType.ERROR);
                    return null;
            }
        }

        switch (lockableEntity.LockState)
        {
            case LockState.CLOSED:
                lockableEntity.LockState = LockState.OPEN;
                player.SendNotification("Das Schloss wurde aufgeschlossen.", NotificationType.INFO);
                break;
            case LockState.OPEN:
                lockableEntity.LockState = LockState.CLOSED;
                player.SendNotification("Das Schloss wurde abgeschlossen.", NotificationType.INFO);
                break;
        }

        switch (lockableEntity)
        {
            case HouseModel h:
                await _houseService.Update(h);
                break;
            case DoorModel d:
                _doorSyncModule.UpdateHouseDoor(d);
                await _doorService.Update(d);
                break;
            case PlayerVehicleModel v:
                await HandleVehicleLock(v);
                break;
        }

        return lockableEntity.LockState;
    }

    private async Task HandleVehicleLock(PlayerVehicleModel vehicleModel)
    {
        await _vehicleService.Update(vehicleModel);

        var serverVehicle = Alt.GetAllVehicles().FindByDbId(vehicleModel.Id);
        if (serverVehicle == null)
        {
            return;
        }

        switch (vehicleModel.LockState)
        {
            case LockState.OPEN:
                await serverVehicle.SetLockStateAsync(VehicleLockState.Unlocked);
                break;
            case LockState.CLOSED:
                await serverVehicle.SetLockStateAsync(VehicleLockState.Locked);
                break;
        }

        await _vehicleModule.SetSyncedDataAsync(serverVehicle);
    }
}