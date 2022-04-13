using AltV.Net;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Housing;
using Server.Modules.WorldMarket;

namespace Server.Handlers.Vehicle;

public class VehicleRefuelHandler : ISingletonScript
{
    private readonly HouseService _houseService;

    private readonly VehicleCatalogService _vehicleCatalogService;
    private readonly WorldMarketModule _worldMarketModule;

    public VehicleRefuelHandler(
        WorldMarketModule worldMarketModule,
        VehicleCatalogService vehicleCatalogService,
        HouseService houseService)
    {
        _worldMarketModule = worldMarketModule;

        _vehicleCatalogService = vehicleCatalogService;
        _houseService = houseService;

        AltAsync.OnClient<ServerPlayer, int>("vehiclemenu:requestrefuel", OnRequestRefuel);
    }

    private async void OnRequestRefuel(ServerPlayer player, int vehicleDbId)
    {
        if (!player.Exists)
        {
            return;
        }

        var vehicle = Alt.GetAllVehicles().FindByDbId(vehicleDbId);
        if (vehicle is not { Exists: true } || vehicle.DbEntity == null)
        {
            player.SendNotification("Dieses Fahrzeug kannst du nicht betanken.", NotificationType.ERROR);
            return;
        }

        if (vehicle.DbEntity.LockState == LockState.CLOSED)
        {
            player.SendNotification("Das Fahrzeug muss aufgeschlossen sein.", NotificationType.ERROR);
            return;
        }

        if (vehicle.EngineOn)
        {
            player.SendNotification("Der Motor des Fahrzeuges muss abgeschaltet sein.", NotificationType.ERROR);
            return;
        }

        var catalogVehicle = await _vehicleCatalogService.GetByKey(vehicle.DbEntity.Model.ToLower());
        if (catalogVehicle == null)
        {
            return;
        }
        
        if (catalogVehicle.MaxTank - vehicle.Fuel < 1)
        {
            player.SendNotification("Dieses Fahrzeug kann nicht weiter aufgetankt werden.", NotificationType.ERROR);
            return;
        }

        player.SetData("INTERACT_VEHICLE_REFILL", vehicleDbId);

        // Check if user is nearby an gas station
        if (await _houseService.GetByDistance(player.Position, 10) is LeaseCompanyHouseModel { LeaseCompanyType: LeaseCompanyType.GAS_STATION })
        {
            var diff = catalogVehicle.MaxTank - vehicle.Fuel;
            float price = _worldMarketModule.FuelPrice[catalogVehicle.FuelType];

            player.EmitLocked("gasstation:openrefuelmenu", diff, price);
            return;
        }

        player.SendNotification("Hier kannst du nicht tanken.", NotificationType.ERROR);
    }
}