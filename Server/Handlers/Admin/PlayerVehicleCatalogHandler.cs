using System.Collections.Generic;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;

namespace Server.Handlers.Admin;

public class PlayerVehicleCatalogHandler : ISingletonScript
{
    private readonly VehicleCatalogService _vehicleCatalogService;
    private readonly VehicleService _vehicleService;
    
    public PlayerVehicleCatalogHandler(
        VehicleCatalogService vehicleCatalogService, 
        VehicleService vehicleService)
    {
        _vehicleCatalogService = vehicleCatalogService;
        _vehicleService = vehicleService;
        
        AltAsync.OnClient<ServerPlayer>("playervehiclecatalog:open", OnOpen);
    }

    private async void OnOpen(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!player.AccountModel.Permission.HasFlag(Permission.STAFF))
        {
            return;
        }

        var vehicles = await _vehicleService.GetAll();
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
                Model = vehicle.Model,
                DisplayName = catalogVehicle.DisplayName,
                DisplayClass = catalogVehicle.DisplayClass,
                CharacterId = vehicle.CharacterModelId ?? -1,
                CharacterName = vehicle.CharacterModel?.Name ?? string.Empty,
            });
        }

        player.EmitGui("playervehiclecatalog:setup", vehicleDatas);
    }
}