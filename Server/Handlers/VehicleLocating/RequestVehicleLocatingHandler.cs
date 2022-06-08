using System.Collections.Generic;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Models;
using Server.DataAccessLayer.Services;

namespace Server.Handlers.VehicleLocating;

public class RequestVehicleLocatingHandler : ISingletonScript
{
    private readonly VehicleCatalogService _vehicleCatalogService;
    private readonly VehicleService _vehicleService;
    private readonly GroupService _groupService;

    public RequestVehicleLocatingHandler(
        VehicleCatalogService vehicleCatalogService,
        VehicleService vehicleService,
        GroupService groupService)
    {
        _vehicleCatalogService = vehicleCatalogService;
        _vehicleService = vehicleService;
        _groupService = groupService;

        AltAsync.OnClient<ServerPlayer>("locating:requestapp", OnRequestApp);
    }

    private async void OnRequestApp(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        var ownedVehicles = await _vehicleService.Where(v => v.CharacterModelId == player.CharacterModel.Id);

        var ownedGroups = await _groupService.GetByOwner(player.CharacterModel.Id);
        foreach (var ownedGroup in ownedGroups)
        {
            ownedVehicles.AddRange(await _vehicleService.Where(v => v.GroupModelOwnerId == ownedGroup.Id));
        }

        var vehicleDatas = new List<VehicleData>();

        foreach (var vehicle in ownedVehicles)
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
                DisplayClass = catalogVehicle.DisplayClass,
                IsGroupVehicle = vehicle.GroupModelOwnerId.HasValue
            });
        }

        player.EmitGui("locating:setvehicles", vehicleDatas);
    }
}