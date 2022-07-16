using AltV.Net;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Modules.Vehicles;

namespace Server.Handlers.Company.PlayerVehicleWorkshop;

public class CloseVehicleServiceHandler : ISingletonScript
{
    private readonly VehicleService _vehicleService;
    private readonly VehicleModule _vehicleModule;
    
    public CloseVehicleServiceHandler(VehicleService vehicleService, VehicleModule vehicleModule)
    {
        _vehicleService = vehicleService;
        _vehicleModule = vehicleModule;

        AltAsync.OnClient<ServerPlayer>("vehicleservice:close", OnExecute);
    }

    private async void OnExecute(ServerPlayer player)
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
        
        player.Dimension = 0;
        await _vehicleModule.Create(playerVehicleModel);
        
        player.DeleteData("VEHICLE_SERVICE_DATA");
        player.DeleteData("VEHICLE_SERVICE_COMPANY_ID");
    }
}