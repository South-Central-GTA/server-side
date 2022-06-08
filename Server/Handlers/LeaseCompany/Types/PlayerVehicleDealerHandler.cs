using System;
using System.Collections.Generic;
using System.Linq;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Group;
using Server.Modules.Group;
using Server.Modules.Vehicles;

namespace Server.Handlers.LeaseCompany.Types;

public class PlayerVehicleDealerHandler : ISingletonScript
{
    private readonly GroupModule _groupModule;
    private readonly GroupService _groupService;
    private readonly VehicleCatalogService _vehicleCatalogService;

    private readonly VehicleModule _vehicleModule;
    private readonly VehicleService _vehicleService;

    public PlayerVehicleDealerHandler(
        VehicleService vehicleService,
        VehicleCatalogService vehicleCatalogService,
        GroupService groupService,
        VehicleModule vehicleModule,
        GroupModule groupModule)
    {
        _vehicleService = vehicleService;
        _vehicleCatalogService = vehicleCatalogService;
        _groupService = groupService;

        _vehicleModule = vehicleModule;
        _groupModule = groupModule;

        AltAsync.OnClient<ServerPlayer, int>("groupmenuvehicledealer:getvehicles", OnRequestVehicles);
        AltAsync.OnClient<ServerPlayer, int>("groupmenuvehicledealer:checkvehicle", OnCheckVehicle);
        AltAsync.OnClient<ServerPlayer, int, int>("groupmenuvehicledealer:spawnvehicle", OnRequestSpawnVehicle);
        AltAsync.OnClient<ServerPlayer, int>("groupmenuvehicledealer:requeststorevehicle", OnRequestStoreVehicle);
    }

    private async void OnRequestVehicles(ServerPlayer player, int groupId)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!player.Exists)
        {
            return;
        }

        var groups = await _groupService.GetGroupsByCharacter(player.CharacterModel.Id);
        var group = groups?.FirstOrDefault(g => g.GroupType == GroupType.COMPANY);
        if (group == null)
        {
            return;
        }

        var companyGroup = (CompanyGroupModel)group;

        if (!await _groupModule.HasPermission(player.CharacterModel.Id, group.Id, GroupPermission.STORE_VEHICLES))
        {
            player.EmitGui("groupmenuvehicledealer:nopermissions");
            return;
        }

        var vehicles = await _vehicleService.Where(v =>
                                                       v.GroupModelOwnerId == groupId &&
                                                       v.VehicleState == VehicleState.IN_STORAGE);

        var vehicleDatas = new List<VehicleData>();

        foreach (var vehicle in vehicles)
        {
            var catalogVehicle = await _vehicleCatalogService.GetByKey(vehicle.Model.ToLower());
            if (catalogVehicle == null)
            {
                continue;
            }

            vehicleDatas.Add(new VehicleData
            {
                Id = vehicle.Id,
                DisplayName = catalogVehicle.DisplayName,
                DisplayClass = catalogVehicle.DisplayClass
            });
        }

        player.EmitGui("groupmenuvehicledealer:sendvehicles", vehicleDatas);
    }

    private async void OnCheckVehicle(ServerPlayer player, int groupId)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!player.IsInVehicle)
        {
            return;
        }

        var groups = await _groupService.GetGroupsByCharacter(player.CharacterModel.Id);
        var group = groups?.FirstOrDefault(g => g.GroupType == GroupType.COMPANY);
        if (group == null)
        {
            return;
        }

        if (!await _groupModule.HasPermission(player.CharacterModel.Id, group.Id, GroupPermission.STORE_VEHICLES))
        {
            return;
        }

        var vehicle = (ServerVehicle)player.Vehicle;
        if (!vehicle.Exists || vehicle.DbEntity == null)
        {
            return;
        }

        if (!vehicle.DbEntity.GroupModelOwnerId.HasValue)
        {
            return;
        }

        if (vehicle.DbEntity.GroupModelOwnerId != groupId)
        {
            return;
        }

        var catalogVehicle = await _vehicleCatalogService.GetByKey(vehicle.DbEntity.Model.ToLower());
        if (catalogVehicle == null)
        {
            return;
        }

        var vehicleData = new VehicleData
        {
            Id = vehicle.DbEntity.Id,
            DisplayName = catalogVehicle.DisplayName,
            DisplayClass = catalogVehicle.DisplayClass
        };

        player.EmitGui("groupmenuvehicledealer:showparkinbutton", vehicleData);
    }

    private async void OnRequestSpawnVehicle(ServerPlayer player, int vehicleDbId, int groupId)
    {
        if (!player.Exists)
        {
            return;
        }

        var groups = await _groupService.GetGroupsByCharacter(player.CharacterModel.Id);
        var group = groups?.FirstOrDefault(g => g.GroupType == GroupType.COMPANY);
        if (group == null)
        {
            return;
        }

        if (!await _groupModule.HasPermission(player.CharacterModel.Id, group.Id, GroupPermission.STORE_VEHICLES))
        {
            return;
        }

        var vehicle = await _vehicleService.GetByKey(vehicleDbId);
        if (vehicle == null)
        {
            return;
        }

        if (vehicle.GroupModelOwnerId.HasValue
            && vehicle.GroupModelOwnerId.Value != groupId)
        {
            return;
        }

        if (vehicle.VehicleState != VehicleState.IN_STORAGE)
        {
            player.SendNotification("Das Fahrzeug wurde gerade ausgeparkt.", NotificationType.ERROR);
            return;
        }

        vehicle.Position = player.Position;
        vehicle.Rotation = player.Rotation;
        vehicle.VehicleState = VehicleState.SPAWNED;

        await _vehicleService.Update(vehicle);

        var serverVehicle = await _vehicleModule.Create(vehicle);
        await player.SetIntoVehicleAsync(serverVehicle, 1);

        UpdateGroupOrdersUi(group);
    }

    private async void OnRequestStoreVehicle(ServerPlayer player, int groupId)
    {
        if (!player.Exists)
        {
            return;
        }

        var groups = await _groupService.GetGroupsByCharacter(player.CharacterModel.Id);
        var group = groups?.FirstOrDefault(g => g.GroupType == GroupType.COMPANY);
        if (group == null)
        {
            return;
        }

        var companyGroup = (CompanyGroupModel)group;

        if (!await _groupModule.HasPermission(player.CharacterModel.Id, group.Id, GroupPermission.STORE_VEHICLES))
        {
            return;
        }

        if (!player.IsInVehicle || player.Seat != 1)
        {
            var gender = player.CharacterModel.Gender == GenderType.MALE ? "der Fahrer" : "die Fahrerin";
            player.SendNotification($"Dein Charakter muss {gender} des Fahrzeug sein.", NotificationType.ERROR);
            return;
        }

        var vehicle = (ServerVehicle)player.Vehicle;
        vehicle.DbEntity.Position = Position.Zero;
        vehicle.DbEntity.Rotation = Rotation.Zero;
        vehicle.DbEntity.VehicleState = VehicleState.IN_STORAGE;

        await _vehicleService.Update(vehicle.DbEntity);

        await vehicle.RemoveAsync();

        UpdateGroupOrdersUi(group);
    }

    private void UpdateGroupOrdersUi(GroupModel groupModel)
    {
        var groupPlayers = Alt.GetAllPlayers()
                              .Where(p => p.Exists && p.IsSpawned && groupModel.Members
                                                                               .Any(m => m.CharacterModelId ==
                                                                                         p.CharacterModel.Id));

        var callback = new Action<ServerPlayer>(async player =>
        {
            if (await _groupModule.HasPermission(player.CharacterModel.Id,
                                                 groupModel.Id,
                                                 GroupPermission.STORE_VEHICLES))
            {
                var vehicles = await _vehicleService.Where(v =>
                                                               v.GroupModelOwnerId == groupModel.Id &&
                                                               v.VehicleState == VehicleState.IN_STORAGE);

                var vehicleDatas = new List<VehicleData>();

                foreach (var vehicle in vehicles)
                {
                    var catalogVehicle = await _vehicleCatalogService.GetByKey(vehicle.Model.ToLower());
                    if (catalogVehicle == null)
                    {
                        continue;
                    }

                    vehicleDatas.Add(new VehicleData
                    {
                        Id = vehicle.Id,
                        DisplayName = catalogVehicle.DisplayName,
                        DisplayClass = catalogVehicle.DisplayClass
                    });
                }

                player.EmitGui("groupmenuvehicledealer:sendvehicles", vehicleDatas);
            }
            else
            {
                player.EmitGui("groupmenuvehicledealer:nopermissions");
            }
        });

        groupPlayers.ForEach(callback);
    }
}