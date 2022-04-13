using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Modules.Group;
using Server.Modules.Vehicles;

namespace Server.Handlers.Group;

public class GroupVehicleOwnershipHandler : ISingletonScript
{
    private readonly GroupModule _groupModule;
    private readonly GroupService _groupService;
    private readonly VehicleModule _vehicleModule;
    private readonly VehicleService _vehicleService;

    public GroupVehicleOwnershipHandler(
        GroupService groupService,
        VehicleService vehicleService,
        GroupModule groupModule,
        VehicleModule vehicleModule)
    {
        _groupService = groupService;
        _vehicleService = vehicleService;

        _groupModule = groupModule;
        _vehicleModule = vehicleModule;

        AltAsync.OnClient<ServerPlayer>("group:switchtoprivatevehicle", OnSwitchToPrivatVehicle);
        AltAsync.OnClient<ServerPlayer, int>("group:switchtogroupvehicle", OnSwitchToGroupVehicle);
    }

    private async void OnSwitchToPrivatVehicle(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!player.IsInVehicle)
        {
            player.SendNotification("Dein Charakter muss in einem Fahrzeug sitzen.", NotificationType.ERROR);
            return;
        }

        var vehicle = (ServerVehicle)player.Vehicle;
        if (!vehicle.DbEntity.GroupModelOwnerId.HasValue)
        {
            player.SendNotification("Das Fahrzeug ist schon ein Privatfahrzeug.", NotificationType.ERROR);
            return;
        }

        var group = await _groupService.GetByKey(vehicle.DbEntity.GroupModelOwnerId.Value);
        if (group == null)
        {
            return;
        }
        
        if (!_groupModule.IsOwner(player, group))
        {
            player.SendNotification("Dein Charakter ist nicht der Eigentümer von dieser Gruppe.", NotificationType.ERROR);
            return;
        }

        if (vehicle.DbEntity.GroupModelOwnerId.Value != group.Id)
        {
            player.SendNotification("Das Fahrzeug gehört zu einer anderen Gruppe.", NotificationType.ERROR);
            return;
        }

        vehicle.DbEntity.GroupModelOwnerId = null;
        vehicle.DbEntity.CharacterModelId = player.CharacterModel.Id;

        await _vehicleService.Update(vehicle.DbEntity);

        await _vehicleModule.SetSyncedDataAsync(vehicle);

        player.SendNotification("Fahrzeug erfolgreich zum Privatfahrzeug gemacht.", NotificationType.SUCCESS);
    }

    private async void OnSwitchToGroupVehicle(ServerPlayer player, int groupId)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!player.IsInVehicle)
        {
            player.SendNotification("Dein Charakter muss in einem Fahrzeug sitzen.", NotificationType.ERROR);
            return;
        }

        var vehicle = (ServerVehicle)player.Vehicle;
        if (!vehicle.DbEntity.CharacterModelId.HasValue)
        {
            player.SendNotification("Das Fahrzeug ist schon ein Gruppenfahrzeug.", NotificationType.ERROR);
            return;
        }

        var group = await _groupService.GetByKey(groupId);
        if (!_groupModule.IsOwner(player, group))
        {
            player.SendNotification("Dein Charakter ist nicht der Eigentümer von dieser Gruppe.", NotificationType.ERROR);
            return;
        }

        if (player.CharacterModel.Id != vehicle.DbEntity.CharacterModelId.Value)
        {
            player.SendNotification("Dein Charakter ist nicht der Eigentümer von diesem Fahrzeug.", NotificationType.ERROR);
            return;
        }

        vehicle.DbEntity.CharacterModelId = null;
        vehicle.DbEntity.GroupModelOwnerId = group.Id;

        await _vehicleService.Update(vehicle.DbEntity);

        await _vehicleModule.SetSyncedDataAsync(vehicle);

        player.SendNotification("Fahrzeug erfolgreich zum Gruppenfahrzeug gemacht.", NotificationType.SUCCESS);
    }
}