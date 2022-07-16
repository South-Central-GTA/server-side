using System;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Helper;
using Server.Modules.Bank;
using Server.Modules.Vehicles;

namespace Server.Handlers.Company.PlayerVehicleWorkshop;

public class BuyWithBankHandler : ISingletonScript
{
    private readonly PlayerVehicleWorkshopModule _playerVehicleWorkshopModule;
    private readonly CompanyGroupService _companyGroupService;
    private readonly VehicleService _vehicleService;
    private readonly VehicleModule _vehicleModule;
    private readonly BankModule _bankModule;
    private readonly TuningModule _tuningModule;
    private readonly PlayerVehicleWorkshopModule _vehicleWorkshopModule;
    private readonly VehicleCatalogService _vehicleCatalogService;
    private readonly BankAccountService _bankAccountService;

    public BuyWithBankHandler(PlayerVehicleWorkshopModule playerVehicleWorkshopModule, CompanyGroupService companyGroupService, 
        VehicleService vehicleService, VehicleModule vehicleModule, TuningModule tuningModule, 
        PlayerVehicleWorkshopModule vehicleWorkshopModule, VehicleCatalogService vehicleCatalogService, BankModule bankModule, 
        BankAccountService bankAccountService)
    {
        _playerVehicleWorkshopModule = playerVehicleWorkshopModule;
        _companyGroupService = companyGroupService;
        _vehicleService = vehicleService;
        _vehicleModule = vehicleModule;
        _tuningModule = tuningModule;
        _vehicleWorkshopModule = vehicleWorkshopModule;
        _vehicleCatalogService = vehicleCatalogService;
        _bankModule = bankModule;
        _bankAccountService = bankAccountService;

        AltAsync.OnClient<ServerPlayer, int>("playervehicleworkshop:buywithbank", OnExecute);
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
            
            var bankAccount = await _bankAccountService.GetByGroup(companyGroup.Id);

            if (!await _bankModule.HasPermission(player, bankAccount, BankingPermission.TRANSFER))
            {
                player.SendNotification("Dein Charakter hat für dieses Konto keine Berechtigung zum Überweisen.", NotificationType.ERROR);
                await _vehicleModule.Create(playerVehicleModel);
                return;
            }
            
            var costs = _vehicleWorkshopModule.CalculatePrice(catalogVehicle.Price, vehicleServiceData.Orders);
            var success = await _bankModule.Withdraw(bankAccount, costs, false,
                $"{companyGroup.Name} - Fahrzeug Tuning für {catalogVehicle.DisplayName}");
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
                player.SendNotification("Das Konto hat nicht genug Guthaben drauf.", NotificationType.ERROR);
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