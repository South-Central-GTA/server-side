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

public class BuyWithCashHandler : ISingletonScript
{
    private readonly PlayerVehicleWorkshopModule _playerVehicleWorkshopModule;
    private readonly CompanyGroupService _companyGroupService;
    private readonly VehicleService _vehicleService;
    private readonly VehicleModule _vehicleModule;
    private readonly MoneyModule _moneyModule;
    private readonly TuningModule _tuningModule;
    private readonly PlayerVehicleWorkshopModule _vehicleWorkshopModule;
    private readonly VehicleCatalogService _vehicleCatalogService;

    public BuyWithCashHandler(PlayerVehicleWorkshopModule playerVehicleWorkshopModule, CompanyGroupService companyGroupService, 
        VehicleService vehicleService, VehicleModule vehicleModule, TuningModule tuningModule, 
        PlayerVehicleWorkshopModule vehicleWorkshopModule, VehicleCatalogService vehicleCatalogService, MoneyModule moneyModule)
    {
        _playerVehicleWorkshopModule = playerVehicleWorkshopModule;
        _companyGroupService = companyGroupService;
        _vehicleService = vehicleService;
        _vehicleModule = vehicleModule;
        _tuningModule = tuningModule;
        _vehicleWorkshopModule = vehicleWorkshopModule;
        _vehicleCatalogService = vehicleCatalogService;
        _moneyModule = moneyModule;

        AltAsync.OnClient<ServerPlayer, int>("playervehicleworkshop:buywithcash", OnExecute);
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
            
            if (!player.GetData("VEHICLE_SERVICE_COMPANY_ID", out int companyId))
            {
                return;
            }

            var companyGroup = await _companyGroupService.GetByKey(companyId);
            if (companyGroup == null)
            {
                return;
            }
            
            var playerVehicleModel = await _vehicleService.GetByKey(vehicleServiceData.VehicleDbId);
            if (playerVehicleModel == null)
            {
                return;
            }
            
            var catalogVehicle = await _vehicleCatalogService.GetByKey(playerVehicleModel.Model.ToLower());
            if (catalogVehicle == null)
            {
                return;
            }
            
            var products = _vehicleWorkshopModule.CalculateProducts(catalogVehicle.Price, vehicleServiceData.Orders);
            if (companyGroup.Products < products)
            {
                player.SendNotification("Unternehmen hat nicht genug Produkte.", NotificationType.ERROR);
                
                player.DeleteData("VEHICLE_SERVICE_DATA");
                player.DeleteData("VEHICLE_SERVICE_COMPANY_ID");

                await _vehicleModule.Create(playerVehicleModel);
                return;
            }
            
            player.DeleteData("VEHICLE_SERVICE_DATA");
            player.DeleteData("VEHICLE_SERVICE_COMPANY_ID");

            var costs = _vehicleWorkshopModule.CalculatePrice(catalogVehicle.Price, vehicleServiceData.Orders);
            var success = await _moneyModule.WithdrawAsync(player, costs);
            if (success)
            {
                foreach (var order in vehicleServiceData.Orders)
                {
                    _tuningModule.TuneVehicle(playerVehicleModel, order.Type, order.Value);
                }

                await _vehicleService.Update(playerVehicleModel);
                
                companyGroup.Products -= products;

                await _companyGroupService.Update(companyGroup);
                _playerVehicleWorkshopModule.UpdateCompanyProductsCount(companyGroup);
                
                player.SendNotification("Fahrzeug Tuning bezahlt und eingebaut.", NotificationType.SUCCESS);
            }
            else
            {
                player.SendNotification("Dein Charakter hat nicht genug Bargeld.", NotificationType.ERROR);
            }
            
            await _vehicleModule.Create(playerVehicleModel);

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}