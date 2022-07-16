using System;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Helper;
using Server.Modules.Money;
using Server.Modules.Vehicles;

namespace Server.Handlers.Company.PlayerVehicleWorkshop;

public class RevokeBuyHandler : ISingletonScript
{
    private readonly VehicleService _vehicleService;
    private readonly VehicleModule _vehicleModule;

    public RevokeBuyHandler(VehicleModule vehicleModule, VehicleService vehicleService)
    {
        _vehicleModule = vehicleModule;
        _vehicleService = vehicleService;
        
        AltAsync.OnClient<ServerPlayer, int>("playervehicleworkshop:revokebuy", OnExecute);
    }

    private async void OnExecute(ServerPlayer player, int bankAccountId)
    {
        try
        {
            if (!player.Exists)
            {
                return;
            }

            if (!player.GetData("VEHICLE_SERVICE_DATA", out VehicleServiceData vehicleServiceData))
            {
                return;
            }
            
            var playerVehicleModel = await _vehicleService.GetByKey(vehicleServiceData.VehicleDbId);
            if (playerVehicleModel == null)
            {
                return;
            }
            
            player.DeleteData("VEHICLE_SERVICE_DATA");
            player.DeleteData("VEHICLE_SERVICE_COMPANY_ID");

            await _vehicleModule.Create(playerVehicleModel);

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}